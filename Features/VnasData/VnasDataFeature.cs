using Coravel.Scheduling.Schedule.Interfaces;
using ZdcReference.FeatureUtilities.Interfaces;
using ZdcReference.Features.VnasData.ScheduledJobs;
using ZdcReference.Features.VnasData.Services;

namespace ZdcReference.Features.VnasData;

public class VnasDataFeature : IServiceConfigurator, ISchedulerConfigurator
{
    public IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddSingleton<CachedVnasDataService>();
        services.AddTransient<FetchAndCacheVnasData>();
        return services;
    }

    public Action<IScheduler> ConfigureScheduler()
    {
        var rnd = new Random();
        return scheduler =>
        {
            scheduler.Schedule<FetchAndCacheVnasData>()
                .HourlyAt(rnd.Next(60))
                .RunOnceAtStart();
        };
    }
}
