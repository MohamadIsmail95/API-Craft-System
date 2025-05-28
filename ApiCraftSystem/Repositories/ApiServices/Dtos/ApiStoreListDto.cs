using ApiCraftSystem.Helper.Enums;

namespace ApiCraftSystem.Repositories.ApiServices.Dtos
{
    public class ApiStoreListDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public ApiMethodeType MethodeType { get; set; }
        public JobPeriodic JobPeriodic { get; set; }

        public ApiStoreListDto() { }

        public ApiStoreListDto(Guid id, string title, string? description, string url, ApiMethodeType methodeType)
        {
            Id = id;
            Title = title;
            Description = description;
            Url = url;
            MethodeType = methodeType;
        }
    }
}
