using Coravel.Scheduling.Schedule.Interfaces;

namespace ZdcReference.FeatureUtilities.Interfaces;

public interface ISchedulerConfigurator
{
    public Action<IScheduler> ConfigureScheduler();
}
