using ApiCraftSystem.Repositories.ApiServices.Dtos;
using ApiCraftSystem.Shared;

namespace ApiCraftSystem.Repositories.ApiServices
{
    public interface IApiService
    {
        Task<PagingResponse> GetListAsync(PagingRequest input);
        Task CreateAsync(ApiStoreDto input);
        Task UpdateAsync(ApiStoreDto input);
        Task<ApiStoreListDto> DeleteAsync(Guid id);
        Task<ApiStoreDto> GetByIdAsync(Guid id);
        Task<bool> FetchAndMap(ApiStoreDto input);
        Task ReCreateDynamicTableAsync(ApiStoreDto input);
    }
}
