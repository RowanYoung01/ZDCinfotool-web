using Coravel.Scheduling.Schedule.Interfaces;
using ZdcReference.Features.Routes.Repositories;
using ZdcReference.Features.Routes.ScheduledJobs;
using ZdcReference.Features.Routes.Services;
using ZdcReference.FeatureUtilities.Interfaces;

namespace ZdcReference.Features.Routes;

public class RoutesModule : IServiceConfigurator, ISchedulerConfigurator
{
    public Action<IScheduler> ConfigureScheduler()
    {
        var rnd = new Random();
        return scheduler =>
        {
            scheduler.Schedule<FetchAndStoreAliasRoutes>()
                .HourlyAt(rnd.Next(60))
                .RunOnceAtStart();

            scheduler.Schedule<FetchAndStoreLoaRules>()
                .HourlyAt(rnd.Next(60))
                .RunOnceAtStart();
        };
    }

    public IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddSingleton<FlightAwareRouteService>();
        services.AddSingleton<AliasRouteRuleRepository>();
        services.AddSingleton<LoaRuleRepository>();
        services.AddTransient<FetchAndStoreAliasRoutes>();
        services.AddTransient<FetchAndStoreLoaRules>();
        services.AddTransient<CskoRouteService>();
        return services;
    }
}
