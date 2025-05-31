using ApiCraftSystem.Repositories.ApiServices.Dtos;

namespace ApiCraftSystem.Shared
{
    public class PagingRequest
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? CurrentSortColumn { get; set; }

        public bool SortDesc = true;

        public string SearchTerm = string.Empty;

    }
}
