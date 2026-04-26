using Coravel.Scheduling.Schedule.Interfaces;
using ZdcReference.Features.Nasr.ScheduledJobs;
using ZdcReference.Features.Nasr.Services;
using ZdcReference.FeatureUtilities.Interfaces;

namespace ZdcReference.Features.Nasr;

public class NasrFeature : IServiceConfigurator, ISchedulerConfigurator
{
    public IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddSingleton<NasrDataService>();
        services.AddTransient<FetchNasrData>();
        return services;
    }

    public Action<IScheduler> ConfigureScheduler()
    {
        var rnd = new Random();
        return scheduler =>
        {
            scheduler.Schedule<FetchNasrData>()
                .DailyAt(7, rnd.Next(60))
                .RunOnceAtStart();
        };
    }
}
