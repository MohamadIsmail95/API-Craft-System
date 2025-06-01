using ApiCraftSystem.Repositories.RateService.Dtos;

namespace ApiCraftSystem.Repositories.RateService
{
    public interface IRateService
    {
        Task CreateRateAsync(RateDto input);
        Task<RateDto> GetUserRateAsync();
    }
}
