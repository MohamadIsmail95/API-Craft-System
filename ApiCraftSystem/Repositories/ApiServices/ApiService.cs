using ApiCraftSystem.Data;
using ApiCraftSystem.Helper.Enums;
using ApiCraftSystem.Model;
using ApiCraftSystem.Repositories.ApiServices.Dtos;
using ApiCraftSystem.Shared;
using AutoMapper;
using Dapper;
using Humanizer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.Common;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ApiCraftSystem.Repositories.ApiServices
{
    public class ApiService : IApiService
    {
        private readonly ApplicationDbContext _db;

        private readonly IMapper _mapper;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly HttpClient _httpClient;

        public ApiService(ApplicationDbContext db, IMapper mapper, IHttpContextAccessor httpContextAccessor,
            HttpClient httpClient)
        {
            _db = db;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
        }

        public async Task CreateAsync(ApiStoreDto input)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            ApiStore api = _mapper.Map<ApiStore>(input);
            api.CreatedAt = DateTime.UtcNow;
            api.CreatedBy = Guid.Parse(userId);
            await _db.ApiStores.AddAsync(api);
            await CreateDynamicTableAsync(input);
            await _db.SaveChangesAsync();


        }
        public async Task<ApiStoreListDto> DeleteAsync(Guid id)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var api = await _db.ApiStores
                .Include(x => x.ApiHeaders)
                .Include(x => x.ApiMaps)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (api == null)
                return null;

            api.IsDeleted = true;
            api.DeletedBy = Guid.Parse(userId);
            api.DeletedAt = DateTime.UtcNow;

            // Handle ApiHeaders soft delete
            if (api.ApiHeaders != null)
            {

                foreach (var header in api.ApiHeaders)
                {

                    header.IsDeleted = true; // soft delete
                }
            }

            // Handle ApiMaps soft delete
            if (api.ApiMaps != null)
            {

                foreach (var map in api.ApiMaps)
                {
                    map.IsDeleted = true;
                }

            }

            await _db.SaveChangesAsync();
            return _mapper.Map<ApiStoreListDto>(api);
        }
        public async Task<ApiStoreDto> GetByIdAsync(Guid id)
        {
            var api = await _db.ApiStores.Include(x => x.ApiHeaders).Include(x => x.ApiMaps).FirstOrDefaultAsync(x => x.Id == id);

            if (api == null)
                return null;

            return _mapper.Map<ApiStoreDto>(api);

        }
        public async Task<PagingResponse> GetListAsync(PagingRequest input)
        {
            var query = _db.ApiStores.AsQueryable();

            // Filter
            if (!string.IsNullOrWhiteSpace(input.SearchTerm))
            {
                input.SearchTerm = input.SearchTerm.ToLower();
                query = query.Where(e => e.Title.ToLower().Contains(input.SearchTerm));

            }

            // Total count for pagination
            var totalCount = await query.CountAsync();

            // Sorting
            if (string.IsNullOrWhiteSpace(input.CurrentSortColumn))
                input.CurrentSortColumn = "CreatedAt";

            var sortOrder = input.SortDesc ? "descending" : "ascending";

            query = query.OrderBy($"{input.CurrentSortColumn} {sortOrder}");

            // Pagination
            query = query.Skip((input.CurrentPage - 1) * input.PageSize).Take(input.PageSize);

            // Projection
            var data = _mapper.Map<List<ApiStoreListDto>>(query);

            return new PagingResponse(data, (int)Math.Ceiling((double)totalCount / input.PageSize), totalCount);


        }
        public async Task UpdateAsync(ApiStoreDto dto)
        {
            // Start a new transaction
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                await UpdateApiStoreInfoAsync(dto);
                await UpdateApiStoreHeadersAsync(dto.Id, dto.ApiHeaders ?? new());
                await UpdateApiStoreMapsAsync(dto.Id, dto.ApiMaps ?? new());
                await CreateDynamicTableAsync(dto);

                // Commit the transaction if everything is successful
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // Rollback if any error occurs
                await transaction.RollbackAsync();
                throw; // Re-throw the exception to handle it further up the stack
            }

        }
        public async Task<bool> FetchAndMap(ApiStoreDto input)
        {
            // Determine HTTP method
            var method = string.Equals(input.MethodeType.ToString(), "POST", StringComparison.OrdinalIgnoreCase)
                ? HttpMethod.Post
                : HttpMethod.Get;

            var handler = new HttpClientHandler();
            if (input.ApiAuthType == ApiAuthType.Windows)
            {
                handler.UseDefaultCredentials = true;
            }

            using var client = new HttpClient(handler);
            var request = new HttpRequestMessage(method, input.Url);

            // Add headers
            if (input.ApiHeaders != null)
            {
                foreach (var header in input.ApiHeaders)
                {
                    request.Headers.TryAddWithoutValidation(header.HeaderKey, header.HeaderValue);
                }
            }

            // Auth
            if (input.ApiAuthType == ApiAuthType.Bearer && !string.IsNullOrWhiteSpace(input.BearerToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", input.BearerToken);
            }

            // Add POST body
            if (method == HttpMethod.Post && !string.IsNullOrWhiteSpace(input.ApiBody))
            {
                request.Content = new StringContent(input.ApiBody, Encoding.UTF8, "application/json");
            }

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return false;

            var content = await response.Content.ReadAsStringAsync();
            var rootJson = JToken.Parse(content);

            // Detect if response is array or single object
            var records = new List<JToken>();

            if (rootJson.Type == JTokenType.Array)
            {
                records = rootJson.ToList();
            }
            else if (rootJson.Type == JTokenType.Object)
            {
                // Optional: use input.ArrayPath if provided
                var arrayPath = input.ApiMaps?.FirstOrDefault(); // Assuming List<KeyValue> with a 'Key' property
                if (arrayPath != null)
                {
                    var path = arrayPath.FromKey;
                    var arrayToken = rootJson.SelectToken(path);

                    if (arrayToken is JArray array)
                        records = array.ToList();
                    else if (arrayToken is JObject obj)
                        records.Add(obj);
                }
                else
                {
                    records.Add(rootJson);
                }
            }

            if (!records.Any()) return false;

            foreach (var item in records)
            {
                var parameters = new DynamicParameters();
                var columns = new List<string>();
                var values = new List<string>();

                foreach (var mapping in input.ApiMaps)
                {
                    var tokenValue = GetNestedToken(item, mapping.FromKey);

                    if (tokenValue != null)
                    {
                        columns.Add(mapping.MapKey);
                        values.Add("@" + mapping.MapKey);
                        parameters.Add(mapping.MapKey, tokenValue.ToString());
                    }
                }

                var sql = $@"
            INSERT INTO {input.TableName}
            ({string.Join(", ", columns)})
            VALUES ({string.Join(", ", values)})
        ";


                //var finalSql = sql;
                //foreach (var paramName in parameters.ParameterNames)
                //{
                //    var value = parameters.Get<dynamic>(paramName);
                //    string formattedValue = value is string || value is DateTime
                //        ? $"'{value}'"
                //        : value?.ToString() ?? "NULL";

                //    finalSql = finalSql.Replace("@" + paramName, formattedValue);
                //}

                //Console.WriteLine("Generated SQL:");
                //Console.WriteLine(finalSql);


                if (string.IsNullOrWhiteSpace(input.ConnectionString))
                    throw new ArgumentException("Connection string cannot be null or empty.");

                if (input.DatabaseType != DatabaseType.SQLServer && input.DatabaseType != DatabaseType.Oracle)
                    throw new NotSupportedException("Unsupported database type.");

                using IDbConnection db = CreateDbConnection(input.DatabaseType, input.ConnectionString);
                await db.ExecuteAsync(sql, parameters);
            }

            return true;
        }

        private static JToken? GetNestedToken(JToken token, string path)
        {
            if (token == null || string.IsNullOrWhiteSpace(path)) return null;

            var segments = path.Split('.');

            foreach (var segment in segments)
            {
                if (token == null) return null;

                // If token is array, take the first object
                if (token.Type == JTokenType.Array)
                {
                    token = token.FirstOrDefault();
                    if (token == null || token.Type != JTokenType.Object)
                        return null;
                }

                // If token is object, try to access property
                if (token.Type == JTokenType.Object)
                {
                    token = token[segment];
                }
                else
                {
                    return null;
                }
            }

            return token;
        }
        private string SanitizeTableName(string tableName)
        {
            // Allow only alphanumeric and underscore
            if (Regex.IsMatch(tableName, @"^[a-zA-Z0-9_]+$"))
                return tableName;

            throw new ArgumentException("Invalid table name.");
        }
        private async Task UpdateApiStoreInfoAsync(ApiStoreDto input)
        {
            var store = await _db.ApiStores.FindAsync(input.Id);
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (store == null)
                throw new Exception("ApiStore not found");

            _mapper.Map(input, store);
            store.ApiHeaders = null;
            store.ApiMaps = null;
            store.UpdatedBy = Guid.Parse(userId);
            store.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        private async Task UpdateApiStoreHeadersAsync(Guid apiStoreId, List<ApiHeaderDto> headers)
        {
            var apiHeaders = await _db.ApiHeaders.Where(x => x.ApiStoreId == apiStoreId).ToListAsync();

            if (headers != null && headers.Count() > 0)
            {
                _db.ApiHeaders.RemoveRange(apiHeaders);

                var newHeaders = _mapper.Map<List<ApiHeader>>(headers);
                await _db.ApiHeaders.AddRangeAsync(newHeaders);
                await _db.SaveChangesAsync();
            }

        }
        private async Task UpdateApiStoreMapsAsync(Guid apiStoreId, List<ApiMapDto> maps)
        {
            var apiMaps = await _db.ApiMaps.Where(x => x.ApiStoreId == apiStoreId).ToListAsync();

            if (maps != null && maps.Count() > 0)
            {
                _db.ApiMaps.RemoveRange(apiMaps);

                var newMaps = _mapper.Map<List<ApiMap>>(maps);
                await _db.ApiMaps.AddRangeAsync(newMaps);
                await _db.SaveChangesAsync();
            }
        }
        private async Task<bool> TableExistsSqlServerAsync(ApiStoreDto input)
        {
            using var connection = new SqlConnection(input.ConnectionString);

            string sql = @"
              SELECT CASE 
              WHEN EXISTS (
              SELECT * FROM INFORMATION_SCHEMA.TABLES 
              WHERE TABLE_NAME = @TableName
              ) 
              THEN 1 ELSE 0 
              END";

            int exists = await connection.ExecuteScalarAsync<int>(sql, new { TableName = input.TableName });
            return exists == 1;
        }
        private async Task<bool> TableExistsOracleAsync(ApiStoreDto input)
        {
            using IDbConnection db = new OracleConnection(input.ConnectionString);

            string sql = @"
            SELECT COUNT(*) 
            FROM USER_TABLES 
            WHERE TABLE_NAME = UPPER(:TableName)";

            int count = await db.ExecuteScalarAsync<int>(sql, new { TableName = input.TableName });
            return count > 0;
        }
        private async Task CreateDynamicTableAsync(ApiStoreDto input)
        {
            if (string.IsNullOrWhiteSpace(input.ConnectionString))
                throw new ArgumentException("Connection string cannot be null or empty.");

            if (input.DatabaseType != DatabaseType.SQLServer && input.DatabaseType != DatabaseType.Oracle)
                throw new NotSupportedException("Unsupported database type.");

            using IDbConnection db = CreateDbConnection(input.DatabaseType, input.ConnectionString);

            string tableName = input.TableName;

            var columnDefinitions = input.ApiMaps.Select(m =>
                $"{EscapeIdentifier(m.MapKey, input.DatabaseType)}  {MapToDbType(m.DataType, input.DatabaseType)}");

            string createTableSql = input.DatabaseType switch
            {
                DatabaseType.SQLServer => $@"
            IF NOT EXISTS (
                SELECT * FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = '{tableName}'
            )
            BEGIN
                CREATE TABLE {tableName} (
                     SId INT PRIMARY KEY IDENTITY(1,1),
                    {string.Join(",\n", columnDefinitions)}
                );
            END",

                DatabaseType.Oracle => $@"
            BEGIN
                EXECUTE IMMEDIATE '
                    CREATE TABLE {tableName} (
                        SId NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                        {string.Join(",", columnDefinitions)}
                    )';
            EXCEPTION
                WHEN OTHERS THEN
                    IF SQLCODE != -955 THEN
                        RAISE;
                    END IF;
            END;",

                _ => throw new NotSupportedException("Unsupported database type.")
            };

            await db.ExecuteAsync(createTableSql);
        }
        private IDbConnection CreateDbConnection(DatabaseType dbType, string connectionString) =>
            dbType switch
            {
                DatabaseType.SQLServer => new SqlConnection(connectionString),
                DatabaseType.Oracle => new OracleConnection(connectionString),
                _ => throw new NotSupportedException()
            };
        private string EscapeIdentifier(string name, DatabaseType dbType)
        {
            // SQL Server uses brackets, Oracle can use double quotes
            return dbType switch
            {
                DatabaseType.SQLServer => $"[{name}]",
                DatabaseType.Oracle => $"\"{name.ToUpper()}\"",
                _ => name
            };
        }
        private string MapToDbType(string dataType, DatabaseType dbType)
        {
            //  dataType = dataType.ToLowerInvariant();

            return dbType switch
            {
                DatabaseType.SQLServer => dataType switch
                {
                    "VarChar" => "VARCHAR(255)",
                    "NVarChar" => "NVARCHAR(255)",
                    "Int" => "Int",
                    "BIT" => "BIT",
                    _ => dataType
                },

                DatabaseType.Oracle => dataType switch
                {
                    "VarChar2" => "VARCHAR2(255)",
                    "NVarChar2" => "NVarChar2(255)",
                    "Number" => "NUMBER(10)",
                    "Boolean" => "NUMBER(1)", // Oracle doesn't have BOOLEAN in tables
                    _ => dataType
                },

                _ => throw new NotSupportedException()
            };
        }

    }
}
