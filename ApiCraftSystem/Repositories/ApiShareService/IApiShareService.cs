using ApiCraftSystem.Model;
using ApiCraftSystem.Repositories.ApiServices.Dtos;
using ApiCraftSystem.Repositories.ApiShareService.Dtos;
using ApiCraftSystem.Repositories.GenericService.Dtos;

namespace ApiCraftSystem.Repositories.ApiShareService
{
    public interface IApiShareService
    {
        Task<ApiShareDto> GetApiShareLink(ApiStoreDto input);
        Task<ApiShare?> GetShareApi(string apiKey, string userId);
        Task<ApiShareDto> CreateShareLink(DynamicTableFormModel input);
    }
}
