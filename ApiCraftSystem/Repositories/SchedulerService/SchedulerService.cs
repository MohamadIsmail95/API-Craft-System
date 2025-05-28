using ApiCraftSystem.Helper.Enums;
using ApiCraftSystem.Repositories.ApiServices;
using ApiCraftSystem.Repositories.ApiServices.Dtos;
using Hangfire;

namespace ApiCraftSystem.Repositories.SchedulerService
{
    public class SchedulerService : ISchedulerService
    {
        private readonly IRecurringJobManager _jobManager;

        public SchedulerService(IRecurringJobManager jobManager)
        {
            _jobManager = jobManager;
        }
        public async Task<string> CreateScheduleAsync(ApiStoreDto input)
        {

            var cron = input.JobPeriodic.ToString() switch
            {
                "Hourly" => Cron.Hourly(),
                "Daily" => Cron.Daily(),
                "Weekly" => Cron.Weekly(),
                "Monthly" => Cron.Monthly(),
                _ => Cron.Daily()
            };

            // Create a custom job ID (e.g., unique per API)
            var jobId = $"fetch-{input.JobPeriodic.ToString()}-store-job-{input.Id}";

            _jobManager.AddOrUpdate<IApiService>(
                jobId,
                service => service.FetchAndMap(input),
                cron
            );

            return jobId;
        }

    }
}
