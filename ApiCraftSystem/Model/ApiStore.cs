using ApiCraftSystem.Helper.Enums;
using System.ComponentModel.DataAnnotations;

namespace ApiCraftSystem.Model
{
    public class ApiStore : FullAudit
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

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

        [Required]
        public ApiMethodeType MethodeType { get; set; } = ApiMethodeType.Get;

        [Required]
        public DatabaseType DatabaseType { get; set; } = DatabaseType.SQLServer;

        public ApiAuthType ApiAuthType { get; set; } = ApiAuthType.None;

        public string? BearerToken { get; set; } = string.Empty;

        public string PrifixRoot { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;


        public virtual ICollection<ApiHeader> ApiHeaders { get; set; }
        public virtual ICollection<ApiMap> ApiMaps { get; set; }
    }
}
