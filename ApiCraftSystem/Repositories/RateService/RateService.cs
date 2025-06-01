using ApiCraftSystem.Data;
using ApiCraftSystem.Model;
using ApiCraftSystem.Repositories.RateService.Dtos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

namespace ApiCraftSystem.Repositories.RateService
{
    public class RateService : IRateService
    {

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RateService(IDbContextFactory<ApplicationDbContext> dbFactory, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _dbFactory = dbFactory;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task CreateRateAsync(RateDto input)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                throw new InvalidOperationException("User is not authenticated.");

            using var db = _dbFactory.CreateDbContext();

            var existingRate = await db.Rates.FirstOrDefaultAsync(x => x.UserId == userId);

            if (existingRate != null)
            {
                db.Rates.Remove(existingRate);

            }

            var newRate = _mapper.Map<Rate>(input);
            newRate.UserId = userId;
            await db.Rates.AddAsync(newRate);
            await db.SaveChangesAsync();
        }

        public async Task<RateDto> GetUserRateAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            using var _db = _dbFactory.CreateDbContext();

            var result = await _db.Rates.FirstOrDefaultAsync(x => x.UserId == userId);

            return _mapper.Map<RateDto>(result);
        }
    }
}
