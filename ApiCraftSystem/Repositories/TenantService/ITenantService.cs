using ApiCraftSystem.Repositories.TenantService.Dto;

namespace ApiCraftSystem.Repositories.TenantService
{
    public interface ITenantService
    {
        Task<List<TenantDto>> GetTenants();
    }
}
