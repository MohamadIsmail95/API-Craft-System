namespace ApiCraftSystem.Shared
{
    public class ShareApiRequest
    {
        public int? PageSize { get; set; } = 10;
        public int? PageIndex { get; set; } = 1;
        public string? OrderBy { get; set; } = string.Empty;
        public bool? Ascending { get; set; } = true;
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

    }
}
