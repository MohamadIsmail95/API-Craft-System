﻿using ApiCraftSystem.Helper.Enums;
using ApiCraftSystem.Helper.Utility;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Dynamic;

namespace ApiCraftSystem.Repositories.GenericService
{
    public class DynamicDataService : IDynamicDataService
    {
        private readonly ILogger<DynamicDataService> _logger;

        public DynamicDataService(ILogger<DynamicDataService> logger)
        {
            _logger = logger;
        }

        public async Task<(List<ExpandoObject> Data, int TotalCount)> GetPagedDataAsync(string connectionString, DatabaseType provider,
            string tableName, string? orderBy, bool? ascending,
            int? pageIndex, int? pageSize, DateTime? dateFrom, DateTime? dateTo, string? dateFilterColumnName)
        {
            try
            {

                string sqlPaged = string.Empty;
                string sqlCount = string.Empty;

                using IDbConnection connection = provider switch
                {
                    DatabaseType.SQLServer => new SqlConnection(connectionString),
                    DatabaseType.Oracle => new OracleConnection(connectionString),
                    _ => throw new NotSupportedException("Unsupported provider.")
                };

                // Try to fallback to a default column like 'Id', 'sid', or discover primary key
                string? defaultOrderColumn = await GetFirstColumnNameAsync(connection, tableName, provider);

                string orderClause = string.IsNullOrWhiteSpace(orderBy)
                    ? $"ORDER BY {defaultOrderColumn}"
                    : $"ORDER BY {orderBy} {(ascending == true ? "ASC" : "DESC")}";


                if (dateFrom != null && dateTo != null && !string.IsNullOrEmpty(dateFilterColumnName))
                {
                    var dateFromStr = ((DateTime)dateFrom).ToString("yyyy-MM-dd HH:mm:ss");
                    var dateToStr = ((DateTime)dateTo).ToString("yyyy-MM-dd HH:mm:ss");

                    sqlPaged = provider switch
                    {
                        DatabaseType.SQLServer =>
                            $"SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderClause}) AS RowNum, * FROM {tableName} WHERE {dateFilterColumnName} BETWEEN '{dateFromStr}' AND '{dateToStr}') AS RowConstrainedResult WHERE RowNum > {pageIndex * pageSize} AND RowNum <= {(pageIndex + 1) * pageSize}",

                        DatabaseType.Oracle =>
                            $@"SELECT * FROM (
                              SELECT a.*, ROWNUM rnum FROM (
                               SELECT *
                               FROM {tableName}
                              WHERE {dateFilterColumnName} BETWEEN 
                              TO_DATE('{dateFromStr}', 'YYYY-MM-DD HH24:MI:SS') AND 
                              TO_DATE('{dateToStr}', 'YYYY-MM-DD HH24:MI:SS')
                              {orderClause}
                              ) a
                               WHERE ROWNUM <= {(pageIndex + 1) * pageSize}
                              ) WHERE rnum > {pageIndex * pageSize}",

                        _ => throw new NotSupportedException("Unsupported provider.")
                    };

                    sqlCount = provider switch
                    {
                        DatabaseType.SQLServer =>
                            $"SELECT COUNT(*) FROM {tableName} WHERE {dateFilterColumnName} BETWEEN '{dateFromStr}' AND '{dateToStr}'",

                        DatabaseType.Oracle =>
                            $@"SELECT COUNT(*) FROM {tableName}
                           WHERE {dateFilterColumnName} BETWEEN 
                          TO_DATE('{dateFromStr}', 'YYYY-MM-DD HH24:MI:SS') AND 
                         TO_DATE('{dateToStr}', 'YYYY-MM-DD HH24:MI:SS')",
                        _ => throw new NotSupportedException("Unsupported provider.")
                    };



                }

                else
                {
                    sqlPaged = provider switch
                    {
                        DatabaseType.SQLServer => $"SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderClause}) AS RowNum, * FROM {tableName}) AS RowConstrainedResult WHERE RowNum > {pageIndex * pageSize} AND RowNum <= {(pageIndex + 1) * pageSize}",
                        DatabaseType.Oracle => $"SELECT * FROM (SELECT a.*, ROWNUM rnum FROM (SELECT * FROM {tableName} {orderClause}) a WHERE ROWNUM <= {(pageIndex + 1) * pageSize}) WHERE rnum > {pageIndex * pageSize}",
                        _ => throw new NotSupportedException("Unsupported provider.")
                    };

                    sqlCount = $"SELECT COUNT(*) FROM {tableName}";

                }

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong in DynamicDataService");
                throw;
            }
        }


        private async Task<string?> GetFirstColumnNameAsync(IDbConnection connection, string tableName, DatabaseType provider)
        {
            string sql = provider switch
            {
                DatabaseType.SQLServer => @"
            SELECT TOP 1 COLUMN_NAME 
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = @TableName 
            ORDER BY ORDINAL_POSITION",

                DatabaseType.Oracle => @"
            SELECT COLUMN_NAME 
            FROM ALL_TAB_COLUMNS 
            WHERE TABLE_NAME = :TableName 
            AND ROWNUM = 1 
            ORDER BY COLUMN_ID",

                _ => throw new NotSupportedException("Unsupported provider.")
            };

            var parameters = new DynamicParameters();
            parameters.Add("TableName", provider == DatabaseType.Oracle ? tableName.ToUpper() : tableName);


            return await connection.QueryFirstOrDefaultAsync<string>(sql, parameters);
        }

    }
}
