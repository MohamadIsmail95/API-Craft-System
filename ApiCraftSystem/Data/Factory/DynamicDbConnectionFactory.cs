using ApiCraftSystem.Helper.Enums;
using Microsoft.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data;
using System.Data.SqlClient;

namespace ApiCraftSystem.Data.Factory
{
    public static class DynamicDbConnectionFactory
    {
        public static IDbConnection CreateConnection(string connectionString, DatabaseType providerType)
        {
            return providerType switch
            {
                DatabaseType.SQLServer => new System.Data.SqlClient.SqlConnection(connectionString),
                DatabaseType.Oracle => new OracleConnection(connectionString),
                _ => throw new NotSupportedException("Unsupported database provider.")
            };
        }
    }
}
