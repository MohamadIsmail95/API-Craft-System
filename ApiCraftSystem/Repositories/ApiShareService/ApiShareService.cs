using ApiCraftSystem.Data;
using ApiCraftSystem.Helper.Utility;
using ApiCraftSystem.Model;
using ApiCraftSystem.Repositories.ApiServices.Dtos;
using ApiCraftSystem.Repositories.ApiShareService.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ApiCraftSystem.Repositories.ApiShareService
{
    public class ApiShareService : IApiShareService
    {

        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        public ApiShareService(ApplicationDbContext db, IConfiguration configuration, IHttpContextAccessor httpContextAccessor,
          UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }
        public async Task<ApiShareDto?> GetApiShareLink(ApiStoreDto input)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return null;

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                return null;

            var serverRoot = _configuration["applicationUrl"];
            string apiKey = Guid.NewGuid().ToString();
            string urlShare = $"{serverRoot}?ApiKey={apiKey}";
            string token = TokenGenerator.GenerateFakeToken(userId, user?.UserName, user?.RoleId, user?.TenantId.ToString());

            ApiShare apiShare = new ApiShare(urlShare, apiKey, token,
                input.ConnectionString, input.DatabaseType, input.TableName);

            apiShare.CreatedBy = Guid.Parse(userId);

            await _db.ApiShares.AddAsync(apiShare);

            await _db.SaveChangesAsync();

            return new ApiShareDto(apiShare.Url, apiShare.ApiToken);
        }

        public async Task<ApiShare?> GetShareApi(string apiKey, string token)
        {
            var result = await _db.ApiShares.FirstOrDefaultAsync(x => x.ApiKey == apiKey && x.ApiToken == token);

            if (result is null)
                return null;

            return result;
        }
    }
}
