using ApiCraftSystem.Model;
using ApiCraftSystem.Repositories.ApiServices.Dtos;
using ApiCraftSystem.Repositories.ApiShareService.Dtos;

namespace ApiCraftSystem.Repositories.ApiShareService
{
    public interface IApiShareService
    {
        Task<ApiShareDto> GetApiShareLink(ApiStoreDto input);
        Task<ApiShare?> GetShareApi(string apiKey, string token);
    }
}
