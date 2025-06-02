using ApiCraftSystem.Helper.Enums;
using Dapper;
using Microsoft.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Dynamic;

namespace ApiCraftSystem.Repositories.GenericService
{
    public class DynamicDataService : IDynamicDataService
    {


        public async Task<(List<ExpandoObject> Data, int TotalCount)> GetPagedDataAsync(string connectionString, DatabaseType provider,
            string tableName, string? orderBy, bool? ascending, int? pageIndex, int? pageSize)
        {
            using IDbConnection connection = provider switch
            {
                DatabaseType.SQLServer => new SqlConnection(connectionString),
                DatabaseType.Oracle => new OracleConnection(connectionString),
                _ => throw new NotSupportedException("Unsupported provider.")
            };

            string orderClause = string.IsNullOrWhiteSpace(orderBy) ? "ORDER BY SId" : $"ORDER BY {orderBy} {(ascending == true ? "ASC" : "DESC")}";
            string sqlPaged = provider switch
            {
                DatabaseType.SQLServer => $"SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderClause}) AS RowNum, * FROM {tableName}) AS RowConstrainedResult WHERE RowNum > {pageIndex * pageSize} AND RowNum <= {(pageIndex + 1) * pageSize}",
                DatabaseType.Oracle => $"SELECT * FROM (SELECT a.*, ROWNUM rnum FROM (SELECT * FROM {tableName} {orderClause}) a WHERE ROWNUM <= {(pageIndex + 1) * pageSize}) WHERE rnum > {pageIndex * pageSize}",
                _ => throw new NotSupportedException("Unsupported provider.")
            };

            string sqlCount = $"SELECT COUNT(*) FROM {tableName}";

            var rows = await connection.QueryAsync<dynamic>(sqlPaged);
            var total = await connection.ExecuteScalarAsync<int>(sqlCount);

            // Convert each DapperRow to ExpandoObject
            var data = new List<ExpandoObject>();
            foreach (var row in rows)
            {
                IDictionary<string, object> expando = new ExpandoObject();
                foreach (var prop in (IDictionary<string, object>)row)
                {
                    expando[prop.Key] = prop.Value;
                }
                data.Add((ExpandoObject)expando);
            }

            return (data, total);
        }


    }
}
