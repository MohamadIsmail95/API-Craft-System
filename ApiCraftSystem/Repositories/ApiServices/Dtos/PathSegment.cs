namespace ApiCraftSystem.Repositories.ApiServices.Dtos
{
    public class PathSegment
    {
        public string Key { get; set; } = string.Empty;
        public string? Map { get; set; } // Can be "*" or null
    }
}
