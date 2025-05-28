using Hangfire;

namespace ApiCraftSystem.HangFire
{
    public class HangfireActivator : JobActivator
    {
        private readonly IServiceProvider _provider;

        public HangfireActivator(IServiceProvider provider)
        {
            _provider = provider;
        }

        public override object ActivateJob(Type jobType)
        {
            return _provider.GetRequiredService(jobType);
        }
    }
}
