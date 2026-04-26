using Coravel.Scheduling.Schedule.Interfaces;
using ZdcReference.Features.Charts.ScheduledJobs;
using ZdcReference.FeatureUtilities.Interfaces;
using ZdcReference.Features.Charts.Services;

namespace ZdcReference.Features.Charts;

public class ChartsFeature : IServiceConfigurator, ISchedulerConfigurator
{
    public IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddSingleton<AviationApiChartService>();
        services.AddSingleton<CifpService>();
        services.AddSingleton<StarApproachConnectionService>();
        services.AddSingleton<PdfRotationDetector>();
        services.AddSingleton<ChartPdfProcessingService>();
        services.AddTransient<FetchAndCacheCharts>();
        return services;
    }

    public Action<IScheduler> ConfigureScheduler()
    {
        var rnd = new Random();
        return scheduler =>
        {
            scheduler.Schedule<FetchAndCacheCharts>()
                .HourlyAt(rnd.Next(60))
                .RunOnceAtStart();
        };
    }
}
