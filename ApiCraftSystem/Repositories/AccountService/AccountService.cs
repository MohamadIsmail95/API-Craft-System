using ApiCraftSystem.Data;
using ApiCraftSystem.Model;
using ApiCraftSystem.Repositories.AccountService.Dtos;
using ApiCraftSystem.Repositories.ApiServices.Dtos;
using ApiCraftSystem.Shared;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Security.Claims;

namespace ApiCraftSystem.Repositories.AccountService
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountService(ApplicationDbContext db, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UserDto> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (user == null || string.IsNullOrEmpty(userId))
                throw new Exception("ApiStore not found");

            user.IsDeleted = true;
            user.DeletedBy = Guid.Parse(userId);
            user.DeletedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> GetByIdAsync(string id)
        {
            var user = await _db.Users.Include(x => x.Role).Include(x => x.Tenant).FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
                throw new Exception("user not found");


            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> GetCurrentUserAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _db.Users.Include(x => x.Role).Include(x => x.Tenant).FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null || string.IsNullOrEmpty(userId))
                throw new Exception("ApiStore not found");

            return _mapper.Map<UserDto>(user);
        }

        public async Task<PagingResponse> GetListAsync(PagingRequest input)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var query = _db.Users.Include(x => x.Tenant).Include(x => x.Role)
                .Where(x => x.Id != userId)
                .AsQueryable();

            query = await FilterTenantAndRole(query);

            if (query == null)
                return new PagingResponse(null, 0, 0);

            // Filter
            if (!string.IsNullOrWhiteSpace(input.SearchTerm))
            {
                input.SearchTerm = input.SearchTerm.ToLower();
                query = query.Where(e => e.UserName.ToLower().Contains(input.SearchTerm));

            }

            // Total count for pagination
            var totalCount = await query.CountAsync();

            // Sorting
            if (string.IsNullOrWhiteSpace(input.CurrentSortColumn))
                input.CurrentSortColumn = "CreatedAt";

            var sortOrder = input.SortDesc ? "descending" : "ascending";

            query = query.OrderBy($"{input.CurrentSortColumn} {sortOrder}");

            // Pagination
            query = query.Skip((input.CurrentPage - 1) * input.PageSize).Take(input.PageSize);

            // Projection
            var data = _mapper.Map<List<UserDto>>(query);

            return new PagingResponse(data, (int)Math.Ceiling((double)totalCount / input.PageSize), totalCount);
        }

        public async Task UpdateAsync(UpdateUserDto input, CancellationToken cancellationToken = default)
        {
            var user = await _db.Users.FindAsync(input.Id);
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (user == null || string.IsNullOrEmpty(userId))
                throw new Exception("user not found");

            _mapper.Map(input, user);
            user.UpdatedBy = Guid.Parse(userId);
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        private async Task<IQueryable<ApplicationUser>> FilterTenantAndRole(IQueryable<ApplicationUser> query)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _db.Users.Where(x => x.Id == userId).Include(x => x.Role).SingleOrDefaultAsync();

            if (user != null)
            {

                if (user?.Role?.Name == "SuperAdmin")
                {
                    return query;
                }

                if (user?.Role?.Name?.ToLower() == "admin")
                {
                    query = query.Where(x => x.TenantId == user.TenantId && x.Role.Name != "SuperAdmin");

                    return query;
                }




            }

            return null;

        }

        public UserDto GetCurrentUser()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = _db.Users.Include(x => x.Role).Include(x => x.Tenant).FirstOrDefault(x => x.Id == userId);

            if (user == null || string.IsNullOrEmpty(userId))
                return null;

            return _mapper.Map<UserDto>(user);
        }

    }
}
