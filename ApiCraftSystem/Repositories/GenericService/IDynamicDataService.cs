using ApiCraftSystem.Helper.Enums;
using System.Dynamic;

namespace ApiCraftSystem.Repositories.GenericService
{
    public interface IDynamicDataService
    {
        Task<(List<ExpandoObject> Data, int TotalCount)> GetPagedDataAsync(string connectionString, DatabaseType provider,
            string tableName, string? orderBy, bool? ascending, int? pageIndex, int? pageSize);
    }
}
