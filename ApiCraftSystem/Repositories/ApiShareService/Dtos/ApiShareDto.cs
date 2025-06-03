namespace ApiCraftSystem.Repositories.ApiShareService.Dtos
{
    public class ApiShareDto
    {
        public string Url { get; set; } = string.Empty;

        public ApiShareDto() { }

        public ApiShareDto(string url)
        {
            Url = url;
        }
    }
}
