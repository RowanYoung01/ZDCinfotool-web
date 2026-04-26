using Coravel.Scheduling.Schedule.Interfaces;
using ZdcReference.Features.Docs.Repositories;
using ZdcReference.Features.Docs.ScheduledJobs;
using ZdcReference.Features.Docs.Services;
using ZdcReference.FeatureUtilities.Interfaces;

namespace ZdcReference.Features.Docs;

public class DocsModule : IServiceConfigurator, ISchedulerConfigurator
{
    public IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddSingleton<DocumentRepository>();
        services.AddSingleton<PdfSectionFinder>();
        services.AddSingleton<ProcedureSearchConfig>();
        services.AddSingleton<ProcedureMatcher>();
        services.AddTransient<FetchAndStoreDocs>();
        return services;
    }

    public Action<IScheduler> ConfigureScheduler()
    {
        return scheduler =>
        {
            scheduler.Schedule<FetchAndStoreDocs>()
                .Hourly()
                .RunOnceAtStart();
        };
    }
}
