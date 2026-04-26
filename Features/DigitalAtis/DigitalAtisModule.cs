using Coravel.Scheduling.Schedule.Interfaces;
using ZdcReference.Features.DigitalAtis.Repositories;
using ZdcReference.Features.DigitalAtis.ScheduledJobs;
using ZdcReference.FeatureUtilities.Interfaces;

namespace ZdcReference.Features.DigitalAtis;

public class DigitalAtisModule : IServiceConfigurator, ISchedulerConfigurator
{
    public IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddSingleton<DigitalAtisRepository>();
        services.AddTransient<FetchAndStoreAtis>();
        return services;
    }

    public Action<IScheduler> ConfigureScheduler()
    {
        return scheduler =>
        {
            scheduler.Schedule<FetchAndStoreAtis>()
                .EveryMinute()
                .RunOnceAtStart();
        };
    }
}
