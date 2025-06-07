using ApiCraftSystem.Repositories.AccountService;
using ApiCraftSystem.Repositories.AccountService.Dtos;
using ApiCraftSystem.Repositories.ApiServices.Dtos;
using ApiCraftSystem.Shared;
using Microsoft.AspNetCore.Components;
using Timer = System.Timers.Timer;

namespace ApiCraftSystem.Components.Account.Users
{
    public class AppUserBase : ComponentBase
    {
        [Inject] protected IAccountService _accountService { get; set; }
        protected PagingRequest PagingRequest = new PagingRequest();
        protected PagingResponse pagingResponse = new PagingResponse();
        protected Timer _searchDebounceTimer;
        protected List<UserDto> Users = new List<UserDto>();
        protected bool ShowConfirmation = false;
        protected string? UserId = null;
        protected bool? Result = null;
        protected bool IsFinish = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadUsers(PagingRequest);

        }
        protected async Task LoadUsers(PagingRequest input)
        {
            var result = await _accountService.GetListAsync(input);
            Users = (List<UserDto>)result.Data;
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
            await LoadUsers(PagingRequest);
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
                await LoadUsers(PagingRequest);
            }
        }
        protected async Task NextPage()
        {
            if (PagingRequest.CurrentPage < pagingResponse.TotalPages)
            {
                PagingRequest.CurrentPage++;
                await LoadUsers(PagingRequest);
            }
        }
        protected async Task GoToPage(int page)
        {
            PagingRequest.CurrentPage = page;
            await LoadUsers(PagingRequest);
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
                    await LoadUsers(PagingRequest);
                    StateHasChanged();
                });
            };
            _searchDebounceTimer.AutoReset = false;
            _searchDebounceTimer.Start();
        }
        private async Task Delete(string id)
        {
            var api = await _accountService.DeleteAsync(id);

            if (api == null)
            {
                return;
            }

            await LoadUsers(PagingRequest);
        }
        protected void PromptOperation(string id)
        {
            UserId = id;
            ShowConfirmation = true;
        }
        protected void CloseConfirmation()
        {
            ShowConfirmation = false;
            UserId = null;
        }
        protected async Task ConfirmOperation()
        {
            try
            {
                if (!string.IsNullOrEmpty(UserId))
                {
                    IsFinish = true;
                    await Delete(UserId);

                }

                ShowConfirmation = false;
                UserId = null;
                IsFinish = false;

            }
            catch
            {

                Result = false;
            }

        }


    }
}
