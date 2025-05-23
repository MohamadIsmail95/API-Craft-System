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
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Linq.Dynamic.Core;
using System.Reflection.PortableExecutable;
using System.Security.Claims;

namespace ApiCraftSystem.Repositories.ApiServices
{
    public class ApiService : IApiService
    {
        private readonly ApplicationDbContext _db;

        private readonly IMapper _mapper;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(ApplicationDbContext db, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
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
                $"{EscapeIdentifier(m.MapKey, input.DatabaseType)} {m.DataType}");

            string createTableSql = input.DatabaseType switch
            {
                DatabaseType.SQLServer => $@"
            IF NOT EXISTS (
                SELECT * FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = '{tableName}'
            )
            BEGIN
                CREATE TABLE {tableName} (
                     Id INT PRIMARY KEY IDENTITY(1,1),
                    {string.Join(",\n", columnDefinitions)}
                );
            END",

                DatabaseType.Oracle => $@"
            BEGIN
                EXECUTE IMMEDIATE '
                    CREATE TABLE {tableName} (
                        Id NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
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
            dataType = dataType.ToLowerInvariant();

            return dbType switch
            {
                DatabaseType.SQLServer => dataType switch
                {
                    "string" => "NVARCHAR(255)",
                    "int" => "INT",
                    "datetime" => "DATETIME",
                    "bool" or "boolean" => "BIT",
                    _ => throw new NotSupportedException($"Unsupported data type: {dataType}")
                },

                DatabaseType.Oracle => dataType switch
                {
                    "string" => "NVARCHAR2(255)",
                    "int" => "NUMBER",
                    "datetime" => "DATE",
                    "bool" or "boolean" => "NUMBER(1)", // Oracle doesn't have BOOLEAN in tables
                    _ => throw new NotSupportedException($"Unsupported data type: {dataType}")
                },

                _ => throw new NotSupportedException()
            };
        }

    }
}
