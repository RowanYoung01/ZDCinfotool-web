using ZdcReference.FeatureUtilities.Interfaces;

namespace ZdcReference.Features.PirepEncoder;

public class PirepModule : IServiceConfigurator
{
    public IServiceCollection AddServices(IServiceCollection services) => services;
}
