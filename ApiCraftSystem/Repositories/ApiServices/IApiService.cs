using ApiCraftSystem.Repositories.ApiServices.Dtos;
using ApiCraftSystem.Shared;

namespace ApiCraftSystem.Repositories.ApiServices
{
    public interface IApiService
    {
        Task<PagingResponse> GetListAsync(PagingRequest input);
        Task CreateAsync(ApiStoreDto input, CancellationToken cancellationToken = default);
        Task UpdateAsync(ApiStoreDto input, CancellationToken cancellationToken = default);
        Task<ApiStoreListDto> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApiStoreDto> GetByIdAsync(Guid id);
        Task<bool> FetchAndMap(ApiStoreDto input);
        Task ReCreateDynamicTableAsync(ApiStoreDto input);
    }
}
