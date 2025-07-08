using ApiCraftSystem.Helper.Enums;
using System.ComponentModel.DataAnnotations;

namespace ApiCraftSystem.Repositories.ApiServices.Dtos
{
    public class ApiStoreDto
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;

        [Required]
        public string Url { get; set; } = string.Empty;

        public string? ApiBody { get; set; } = string.Empty;

        public string? ApiResponse { get; set; } = string.Empty;

        public string? ServerIp { get; set; } = string.Empty;

        public string? ServerPort { get; set; } = string.Empty;

        public string? DatabaseName { get; set; } = string.Empty;

        [Required]
        public string TableName { get; set; } = string.Empty;
        public string? DatabaseUserName { get; set; } = string.Empty;
        public string? SchemaName { get; set; } = string.Empty;
        public string? DatabaseUserPassword { get; set; } = string.Empty;

        [Required]
        public string ConnectionString { get; set; } = string.Empty;

        public ApiMethodeType MethodeType { get; set; }

        public ApiAuthType ApiAuthType { get; set; } = ApiAuthType.None;

        public string? BearerToken { get; set; } = string.Empty;

        public string PrifixRoot { get; set; } = string.Empty;

        [Required]
        public DatabaseType DatabaseType { get; set; } = DatabaseType.SQLServer;

        public string? JobId { get; set; } = string.Empty;

        public int? ScHour { get; set; } = null;
        public int? ScMin { get; set; } = null;

        public string? AuthUrl { get; set; } = string.Empty;
        public string? AuthUrlBody { get; set; } = string.Empty;
        public string? AuthHeaderParam { get; set; } = string.Empty;
        public string? AuthResponseParam { get; set; } = string.Empty;
        public ApiMethodeType? AuthMethodeType { get; set; } = ApiMethodeType.Post;
        public Guid? TenantId { get; set; }

        public string BasicUserName { get; set; } = string.Empty;

        public string BasicPassword { get; set; } = string.Empty;

        public List<ApiHeaderDto>? ApiHeaders { get; set; } = new List<ApiHeaderDto>();

        [Required]
        public List<ApiMapDto> ApiMaps { get; set; } = new List<ApiMapDto>();


    }
}
