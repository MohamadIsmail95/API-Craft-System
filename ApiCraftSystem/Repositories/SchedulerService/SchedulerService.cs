using ApiCraftSystem.Helper.Enums;
using ApiCraftSystem.Repositories.ApiServices;
using ApiCraftSystem.Repositories.ApiServices.Dtos;
using Hangfire;
using Hangfire.Storage.Monitoring;

namespace ApiCraftSystem.Repositories.SchedulerService
{
    public class SchedulerService : ISchedulerService
    {
        private readonly IRecurringJobManager _jobManager;

        public SchedulerService(IRecurringJobManager jobManager)
        {
            _jobManager = jobManager;
        }
        public async Task<string?> CreateScheduleAsync(ApiStoreDto input)
        {

            if (input.JobPeriodic == JobPeriodic.NA)
            {
                return null;
            }


            var cron = input.JobPeriodic.ToString() switch
            {
                "Every_5Min" => "*/5 * * * *",
                "Every_10Min" => "*/10 * * * *",
                "Every_15Min" => "*/15 * * * *",
                "Every_30Min" => "*/30 * * * *",
                "Every_45Min" => "*/45 * * * *",
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
        public Task RemoveSchedule(string jobId)
        {
            RecurringJob.RemoveIfExists(jobId);
            return Task.CompletedTask;
        }
        public List<ProcessingJobDto> GetRunningJobs()
        {
            var monitor = JobStorage.Current.GetMonitoringApi();
            var processing = monitor.ProcessingJobs(0, 50); // first 50

            return processing.Select(x => x.Value).ToList();
        }
        public List<SucceededJobDto> GetSuccessJobs()
        {
            var monitor = JobStorage.Current.GetMonitoringApi();
            var succeeded = monitor.SucceededJobs(0, 50); // first 50

            return succeeded.Select(x => x.Value).ToList();
        }
        public List<FailedJobDto> GetFailedJobs()
        {
            var monitor = JobStorage.Current.GetMonitoringApi();
            var failed = monitor.FailedJobs(0, 50);

            return failed.Select(x => x.Value).ToList();
        }

    }
}
