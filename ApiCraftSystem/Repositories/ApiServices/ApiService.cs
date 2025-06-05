using ApiCraftSystem.Data;
using ApiCraftSystem.Helper;
using ApiCraftSystem.Helper.Enums;
using ApiCraftSystem.Model;
using ApiCraftSystem.Repositories.ApiServices.Dtos;
using ApiCraftSystem.Repositories.SchedulerService;
using ApiCraftSystem.Shared;
using AutoMapper;
using Azure.Core;
using Dapper;
using Humanizer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

        private readonly ISchedulerService _schedulerService;

        public ApiService(ApplicationDbContext db, IMapper mapper, IHttpContextAccessor httpContextAccessor,
            HttpClient httpClient, ISchedulerService schedulerService)
        {
            _db = db;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
            _schedulerService = schedulerService;
        }

        public async Task CreateAsync(ApiStoreDto input, CancellationToken cancellationToken = default)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            ApiStore api = _mapper.Map<ApiStore>(input);
            api.CreatedAt = DateTime.UtcNow;
            api.CreatedBy = Guid.Parse(userId);
            if (input.ScHour != null && input.ScMin != null)
            {
                api.JobId = await _schedulerService.CreateScheduleAsync(input);

            }
            await _db.ApiStores.AddAsync(api, cancellationToken);
            await CreateDynamicTableAsync(input);
            await _db.SaveChangesAsync(cancellationToken);


        }
        public async Task<ApiStoreListDto> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var api = await _db.ApiStores
                .Include(x => x.ApiHeaders)
                .Include(x => x.ApiMaps)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (api == null || string.IsNullOrEmpty(userId))
                throw new Exception("ApiStore not found");

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
            if (!string.IsNullOrEmpty(api.JobId))
            {
                await _schedulerService.RemoveSchedule(api.JobId);

            }
            await _db.SaveChangesAsync(cancellationToken);
            return _mapper.Map<ApiStoreListDto>(api);
        }
        public async Task<ApiStoreDto> GetByIdAsync(Guid id)
        {
            var api = await _db.ApiStores.Include(x => x.ApiHeaders).Include(x => x.ApiMaps).FirstOrDefaultAsync(x => x.Id == id);

            if (api == null)
                throw new Exception("ApiStore not found");


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
        public async Task UpdateAsync(ApiStoreDto dto, CancellationToken cancellationToken = default)
        {
            // Start a new transaction
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                await UpdateApiStoreInfoAsync(dto);
                await UpdateApiStoreHeadersAsync(dto.Id, dto.ApiHeaders ?? new());
                await UpdateApiStoreMapsAsync(dto.Id, dto.ApiMaps ?? new());
                await CreateDynamicTableAsync(dto);

                if ((dto.ScHour == null && dto.ScMin == null) && !string.IsNullOrEmpty(dto.JobId))
                {
                    await _schedulerService.RemoveSchedule(dto.JobId);
                }

                // Commit the transaction if everything is successful
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                // Rollback if any error occurs
                await transaction.RollbackAsync(cancellationToken);
                throw; // Re-throw the exception to handle it further up the stack
            }

        }
        public async Task<bool> FetchAndMap(ApiStoreDto input)
        {
            //-----------Call Token From Third Party-----------
            string tokenHeader = string.Empty;

            if (input.ApiAuthType == ApiAuthType.Custom && !string.IsNullOrEmpty(input.AuthUrl) &&
                !string.IsNullOrEmpty(input.AuthHeaderParam) && input.AuthMethodeType != null
                && !string.IsNullOrEmpty(input.AuthResponseParam))
            {
                tokenHeader = await GetThirdPartyAPIToken(input.AuthUrl, input.AuthUrlBody, input.AuthMethodeType, input.AuthResponseParam);
            }

            //------------------------------------------------
            await CreateDynamicTableAsync(input);

            var method = string.Equals(input.MethodeType.ToString(), "POST", StringComparison.OrdinalIgnoreCase)
                ? HttpMethod.Post
                : HttpMethod.Get;

            var handler = new HttpClientHandler();
            if (input.ApiAuthType == ApiAuthType.Windows)
                handler.UseDefaultCredentials = true;

            using var client = new HttpClient(handler);
            var request = new HttpRequestMessage(method, input.Url);

            // Headers
            if (input.ApiHeaders != null)
            {
                foreach (var header in input.ApiHeaders)
                {
                    request.Headers.TryAddWithoutValidation(header.HeaderKey, header.HeaderValue);
                }
            }

            if (!string.IsNullOrEmpty(tokenHeader) && !string.IsNullOrEmpty(input.AuthHeaderParam))
            {
                if (input.AuthHeaderParam.Trim().ToLower() == "bearer")
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenHeader);

                }
                else
                {
                    request.Headers.TryAddWithoutValidation(input.AuthHeaderParam, tokenHeader);

                }

            }

            // Bearer Token Auth
            if (input.ApiAuthType == ApiAuthType.Bearer && !string.IsNullOrWhiteSpace(input.BearerToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", input.BearerToken);
            }

            // Body for POST
            if (method == HttpMethod.Post && !string.IsNullOrWhiteSpace(input.ApiBody))
            {
                request.Content = new StringContent(input.ApiBody, Encoding.UTF8, "application/json");
            }

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return false;

            var content = await response.Content.ReadAsStringAsync();
            var rootJson = JToken.Parse(content);

            // STEP 1: Gather all records from root using unique root paths
            var allRecords = new List<JToken>();
            var rootPaths = input.ApiMaps
                                 .Select(m => ParseObject.ExtractRootPath($"{input.PrifixRoot}.{m.FromKey}"))
                                 .Distinct();

            foreach (var rootPath in rootPaths)
            {
                var tokens = ParseObject.GetRootArray(rootJson, rootPath);
                allRecords.AddRange(tokens);
            }

            if (!allRecords.Any()) return false;

            using IDbConnection db = CreateDbConnection(input.DatabaseType, input.ConnectionString);

            // STEP 2: Iterate each root item and map fields
            foreach (var item in allRecords)
            {
                var parameters = new DynamicParameters();
                var columns = new List<string>();
                var values = new List<string>();

                foreach (var mapping in input.ApiMaps)
                {
                    string prefix = input.PrifixRoot?.Trim('.').Trim() ?? string.Empty;
                    string fromKey = mapping.FromKey?.Trim('.') ?? string.Empty;

                    string concatFromKey = string.IsNullOrWhiteSpace(prefix)
                        ? fromKey
                        : $"{prefix}.{fromKey}";

                    var extracted = ParseObject.ResolveWildcardValues(item, ParseObject.GetLastCleanSegment(concatFromKey));
                    var first = extracted.FirstOrDefault();

                    if (first != null)
                    {
                        columns.Add(mapping.MapKey);
                        values.Add("@" + mapping.MapKey);
                        parameters.Add(mapping.MapKey, first);
                    }
                }

                if (columns.Count == 0) continue;

                var sql = $@"INSERT INTO {input.TableName} ({string.Join(", ", columns)})
                     VALUES ({string.Join(", ", values)})";

                await db.ExecuteAsync(sql, parameters);
            }

            return true;
        }
        public async Task ReCreateDynamicTableAsync(ApiStoreDto input)
        {
            if (string.IsNullOrWhiteSpace(input.ConnectionString))
                throw new ArgumentException("Connection string cannot be null or empty.");

            if (input.DatabaseType != DatabaseType.SQLServer && input.DatabaseType != DatabaseType.Oracle)
                throw new NotSupportedException("Unsupported database type.");

            using IDbConnection db = CreateDbConnection(input.DatabaseType, input.ConnectionString);

            string tableName = EscapeIdentifier(input.TableName, input.DatabaseType);

            var columnDefinitions = input.ApiMaps.Select(m =>
                $"{EscapeIdentifier(m.MapKey, input.DatabaseType)} {MapToDbType(m.DataType, input.DatabaseType)}");

            string createTableSql = input.DatabaseType switch
            {
                DatabaseType.SQLServer => $@"
            IF OBJECT_ID(N'{tableName}', N'U') IS NOT NULL
            BEGIN
                DROP TABLE {tableName};
            END;

            CREATE TABLE {tableName} (
                SId INT PRIMARY KEY IDENTITY(1,1),
                {string.Join(",\n", columnDefinitions)}
            );",

                DatabaseType.Oracle => $@"
            DECLARE
                v_count NUMBER;
            BEGIN
                SELECT COUNT(*) INTO v_count FROM user_tables WHERE table_name = UPPER('{tableName}');
                IF v_count > 0 THEN
                    EXECUTE IMMEDIATE 'DROP TABLE {tableName} CASCADE CONSTRAINTS';
                END IF;

                EXECUTE IMMEDIATE '
                    CREATE TABLE {tableName} (
                        SId NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                        {string.Join(",", columnDefinitions)}
                    )';
            END;",

                _ => throw new NotSupportedException("Unsupported database type.")
            };

            await db.ExecuteAsync(createTableSql);
        }
        private async Task UpdateApiStoreInfoAsync(ApiStoreDto input)
        {
            var store = await _db.ApiStores.FindAsync(input.Id);
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (store == null || string.IsNullOrEmpty(userId))
                throw new Exception("ApiStore not found");

            _mapper.Map(input, store);
            store.ApiHeaders = null;
            store.ApiMaps = null;
            store.UpdatedBy = Guid.Parse(userId);
            store.UpdatedAt = DateTime.UtcNow;
            store.JobId = await _schedulerService.CreateScheduleAsync(input);

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

        private async Task<string> GetThirdPartyAPIToken(string? AuthUrl, string? AuthUrlBody,
            ApiMethodeType? AuthMethodeType, string? AuthResponseParam)
        {
            using var httpClient = new HttpClient();
            HttpResponseMessage loginResponse;

            var method = string.Equals(AuthMethodeType.ToString(), "POST", StringComparison.OrdinalIgnoreCase)
                ? HttpMethod.Post
                : HttpMethod.Get;

            // Body for POST
            if (method == HttpMethod.Post && !string.IsNullOrWhiteSpace(AuthUrlBody))
            {
                var content = new StringContent(AuthUrlBody ?? "", Encoding.UTF8, "application/json");
                loginResponse = await httpClient.PostAsync(AuthUrl, content);
            }

            else if (method == HttpMethod.Get)
            {
                loginResponse = await httpClient.GetAsync(AuthUrl);

            }

            else
            {
                throw new ArgumentException("Unsupported HTTP method");
            }

            // 2. Read and parse login response
            if (!loginResponse.IsSuccessStatusCode)
                throw new Exception($"Login request failed: {loginResponse.StatusCode}");

            var loginJson = await loginResponse.Content.ReadAsStringAsync();

            var token = JObject.Parse(loginJson)[AuthResponseParam]?.ToString();

            if (string.IsNullOrEmpty(token))
                throw new Exception($"Token not found in login response with property name '{AuthResponseParam}'");

            return token;
        }
    }
}
