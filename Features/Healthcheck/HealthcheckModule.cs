using ZdcReference.FeatureUtilities.Interfaces;

namespace ZdcReference.Features.Healthcheck;

public class HealthcheckModule : IServiceConfigurator
{
    public IServiceCollection AddServices(IServiceCollection services)
    {
        return services;
    }
}