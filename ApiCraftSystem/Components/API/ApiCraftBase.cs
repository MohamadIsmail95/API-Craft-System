using ApiCraftSystem.Repositories.ApiServices;
using ApiCraftSystem.Repositories.ApiServices.Dtos;
using ApiCraftSystem.Shared;
using Microsoft.AspNetCore.Components;
using Timer = System.Timers.Timer;

namespace ApiCraftSystem.Components.API
{
    public class ApiCraftBase : ComponentBase
    {
        [Inject] protected IApiService _apiService { get; set; }

        protected PagingRequest PagingRequest = new PagingRequest();
        protected PagingResponse pagingResponse = new PagingResponse();
        protected Timer _searchDebounceTimer;
        protected List<ApiStoreListDto> apis = new List<ApiStoreListDto>();
        protected bool showConfirmation = false;
        protected Guid? deleteId = null;

        protected override async Task OnInitializedAsync()
        {
            await LoadApis(PagingRequest);

        }
        protected async Task LoadApis(PagingRequest input)
        {
            var result = await _apiService.GetListAsync(input);
            apis = (List<ApiStoreListDto>)result.Data;
            pagingResponse.TotalCount = result.TotalCount;
            pagingResponse.TotalPages = (int)Math.Ceiling((double)pagingResponse.TotalCount / input.PageSize);
        }
        protected async Task SortBy(string column)
        {
            if (PagingRequest.CurrentSortColumn == column)
                PagingRequest.SortDesc = !PagingRequest.SortDesc;
            else
            {
                PagingRequest.CurrentSortColumn = column;
                PagingRequest.SortDesc = false;
            }
            PagingRequest.CurrentPage = 1;
            await LoadApis(PagingRequest);
        }
        protected MarkupString SortIcon(string column)
        {
            if (PagingRequest.CurrentSortColumn != column)
                return new MarkupString("");

            return PagingRequest.SortDesc
                ? new MarkupString("<span>&#9660;</span>") // down arrow
                : new MarkupString("<span>&#9650;</span>"); // up arrow
        }
        protected async Task PrevPage()
        {
            if (PagingRequest.CurrentPage > 1)
            {
                PagingRequest.CurrentPage--;
                await LoadApis(PagingRequest);
            }
        }
        protected async Task NextPage()
        {
            if (PagingRequest.CurrentPage < pagingResponse.TotalPages)
            {
                PagingRequest.CurrentPage++;
                await LoadApis(PagingRequest);
            }
        }
        protected async Task GoToPage(int page)
        {
            PagingRequest.CurrentPage = page;
            await LoadApis(PagingRequest);
        }
        protected void OnSearchInput(ChangeEventArgs e)
        {
            PagingRequest.SearchTerm = e.Value?.ToString() ?? "";

            _searchDebounceTimer?.Stop();
            _searchDebounceTimer?.Dispose();
            _searchDebounceTimer = new System.Timers.Timer(500);
            _searchDebounceTimer.Elapsed += async (sender, args) =>
            {
                _searchDebounceTimer?.Stop();
                _searchDebounceTimer?.Dispose();
                _searchDebounceTimer = null;

                await InvokeAsync(async () =>
                {
                    await LoadApis(PagingRequest);
                    StateHasChanged();
                });
            };
            _searchDebounceTimer.AutoReset = false;
            _searchDebounceTimer.Start();
        }
        private async Task Delete(Guid id)
        {
            var api = await _apiService.DeleteAsync(id);

            if (api == null)
            {
                return;
            }

            await LoadApis(PagingRequest);
        }
        protected void PromptDelete(Guid id)
        {
            deleteId = id;
            showConfirmation = true;
        }
        protected void CloseConfirmation()
        {
            showConfirmation = false;
            deleteId = null;
        }
        protected async Task ConfirmDelete()
        {
            if (deleteId.HasValue)
            {
                await Delete(deleteId.Value);
            }

            showConfirmation = false;
            deleteId = null;
        }
    }
}
