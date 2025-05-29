using ApiCraftSystem.Helper.Enums;
using ApiCraftSystem.Repositories.ApiServices.Dtos;
using Hangfire.Storage.Monitoring;

namespace ApiCraftSystem.Repositories.SchedulerService
{
    public interface ISchedulerService
    {
        Task<string?> CreateScheduleAsync(ApiStoreDto input);
        Task RemoveSchedule(string jobId);
        List<ProcessingJobDto> GetRunningJobs();
        public List<SucceededJobDto> GetSuccessJobs();
        public List<FailedJobDto> GetFailedJobs();

    }
}
