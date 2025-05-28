using ApiCraftSystem.Helper.Enums;
using ApiCraftSystem.Repositories.ApiServices.Dtos;

namespace ApiCraftSystem.Repositories.SchedulerService
{
    public interface ISchedulerService
    {
        Task<string> CreateScheduleAsync(ApiStoreDto input);

    }
}
