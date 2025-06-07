using ApiCraftSystem.Repositories.AccountService.Dtos;
using ApiCraftSystem.Repositories.ApiServices.Dtos;
using ApiCraftSystem.Shared;

namespace ApiCraftSystem.Repositories.AccountService
{
    public interface IAccountService
    {
        Task<PagingResponse> GetListAsync(PagingRequest input);
        Task<UserDto> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task UpdateAsync(UpdateUserDto input, CancellationToken cancellationToken = default);
        Task<UserDto> GetByIdAsync(string id);
        Task<UserDto> GetCurrentUserAsync();
        UserDto GetCurrentUser();


    }
}
