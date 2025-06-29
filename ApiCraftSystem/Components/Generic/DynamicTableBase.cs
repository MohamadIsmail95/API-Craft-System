using ApiCraftSystem.Data;
using ApiCraftSystem.Repositories.ApiShareService;
using ApiCraftSystem.Repositories.GenericService;
using ApiCraftSystem.Repositories.GenericService.Dtos;
using ApiCraftSystem.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using OfficeOpenXml;
using System.Dynamic;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace ApiCraftSystem.Components.Generic
{
    public class DynamicTableBase : ComponentBase
    {
        [Inject] protected IDynamicDataService _service { get; set; }
        [Inject] protected IJSRuntime JS { get; set; }
        [Inject] protected UserManager<ApplicationUser> _userManager { get; set; }
        [Inject] protected IApiShareService _apiShareService { get; set; }
        public List<ExpandoObject> Data { get; set; } = new();
        public List<string> Headers { get; set; } = new();
        protected int PageIndex { get; set; } = 1;
        protected int PageSize { get; set; } = 10;
        protected int TotalCount { get; set; } = 0;
        protected string SortColumn { get; set; } = "";
        protected bool SortAscending { get; set; } = true;

        protected DynamicTableFormModel DataCraftForm = new DynamicTableFormModel();

        protected bool? submitted = null;

        protected bool IsLoading;

        public int TotalPages = 0;
        protected List<LookUpDto> UsersLookUp { get; set; } = new();

        protected string ShareLink = string.Empty;

        protected ElementReference userSelectRef;

        protected override async Task OnInitializedAsync()
        {
            await LoadUsersAsync();
        }
        protected async Task LoadData()
        {
            if (DataCraftForm.SelectedProvider == null)
                return;

            try
            {
                IsLoading = true;
                var result = await _service.GetPagedDataAsync(
                    DataCraftForm.ConnectionString,
                    DataCraftForm.SelectedProvider,
                    DataCraftForm.TableName,
                    SortColumn,
                    SortAscending,
                    PageIndex - 1,
                    PageSize, null, null, null
                );
                Data = result.Data;
                TotalCount = result.TotalCount;
                TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);

                Headers = Data.FirstOrDefault() is IDictionary<string, object> row
                    ? row.Keys.ToList()
                    : new List<string>();

                submitted = true;
                IsLoading = false;

            }
            catch (Exception ex)
            {
                submitted = false;
                IsLoading = false;

            }

        }
        protected async Task SortByColumn(string column)
        {
            if (SortColumn == column)
            {
                SortAscending = !SortAscending;
            }
            else
            {
                SortColumn = column;
                SortAscending = true;
            }
            await LoadData();
        }
        protected async Task ExportToExcel()
        {

            var resultExcel = await _service.GetPagedDataAsync(
                DataCraftForm.ConnectionString,
                DataCraftForm.SelectedProvider,
                DataCraftForm.TableName,
                SortColumn,
                SortAscending,
                0,
                10000000, null, null, null
            );
            var ExcelData = resultExcel.Data;

            var ExcelHeaders = Data.FirstOrDefault() is IDictionary<string, object> row
                ? row.Keys.ToList()
                : new List<string>();

            //---------------------------------------------------

            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Export");

            ExcelHeaders = ExcelHeaders.Where(x => x.ToLower() != "sid" && x.ToLower() != "rownum" && x.ToLower() != "rnum").ToList();

            for (int i = 0; i < ExcelHeaders.Count; i++)
            {

                worksheet.Cells[1, i + 1].Value = ExcelHeaders[i];

            }

            for (int row1 = 0; row1 < ExcelData.Count; row1++)
            {
                var rowData = (IDictionary<string, object>)ExcelData[row1];

                for (int col = 0; col < ExcelHeaders.Count; col++)
                {

                    worksheet.Cells[row1 + 2, col + 1].Value = rowData[ExcelHeaders[col]];


                }
            }

            var fileName = $"{DataCraftForm.TableName}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var bytes = package.GetAsByteArray();

            using var streamRef = new MemoryStream(bytes);
            using var dotnetStreamRef = new DotNetStreamReference(streamRef);
            await JS.InvokeVoidAsync("downloadFile", fileName, dotnetStreamRef);
        }
        protected async Task PrevPage()
        {
            if (PageIndex > 1)
            {
                PageIndex--;
                await LoadData();
            }
        }
        protected async Task NextPage()
        {
            if (PageIndex < TotalCount)
            {
                PageIndex++;
                await LoadData();
            }
        }
        protected async Task GoToPage(int page)
        {
            PageIndex = page;
            await LoadData();
        }
        protected async Task LoadUsersAsync()
        {
            List<ApplicationUser> Users = await _userManager.Users.ToListAsync();

            UsersLookUp = Users.Select(x => new LookUpDto
            {
                Id = x.Id,
                Name = x.UserName

            }).ToList();
        }
        protected async Task GetShareLink()
        {
            var apiShare = await _apiShareService.CreateShareLink(DataCraftForm);

            ShareLink = apiShare.Url;


        }
        public async Task CloseConfirmation()
        {
            ShareLink = string.Empty;
        }
        protected async Task OnUsersSelected(ChangeEventArgs e)
        {
            var selectedValues = await JS.InvokeAsync<string[]>("getSelectedValues", userSelectRef);
            DataCraftForm.UserIds = selectedValues.ToList();
        }
    }
}
