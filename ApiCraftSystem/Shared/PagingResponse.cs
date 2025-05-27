namespace ApiCraftSystem.Shared
{
    public class PagingResponse
    {
        public object? Data { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        public PagingResponse() { }
        public PagingResponse(object data, int totalPages, int totalCount)
        {
            Data = data;
            TotalPages = totalPages;
            TotalCount = totalCount;
        }

    }
}
