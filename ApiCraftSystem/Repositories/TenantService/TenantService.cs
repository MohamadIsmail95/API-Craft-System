using ApiCraftSystem.Data;
using ApiCraftSystem.Repositories.TenantService.Dto;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ApiCraftSystem.Repositories.TenantService
{
    public class TenantService : ITenantService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;


        public TenantService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        public async Task<List<TenantDto>> GetTenants()
        {
            var tenants = await _db.Tenants.ToListAsync();

            return _mapper.Map<List<TenantDto>>(tenants);
        }
    }
}
