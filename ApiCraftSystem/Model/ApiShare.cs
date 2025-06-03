using ApiCraftSystem.Helper.Enums;

namespace ApiCraftSystem.Model
{
    public class ApiShare : FullAudit
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Url { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public DatabaseType DatabaseType { get; set; } = DatabaseType.SQLServer;
        public string TableName { get; set; } = string.Empty;
        public string? UserIds { get; set; } = string.Empty;

        public ApiShare() { }

        public ApiShare(string url, string apiKey, string connectionString, DatabaseType databaseType,
            string tableName, string? userIds)
        {
            Id = Guid.NewGuid();
            Url = url;
            ApiKey = apiKey;
            ConnectionString = connectionString;
            DatabaseType = databaseType;
            TableName = tableName;
            UserIds = userIds;
        }
    }
}
