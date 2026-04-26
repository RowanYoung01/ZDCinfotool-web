using Coravel.Invocable;
using ZdcReference.Features.Nasr.Services;

namespace ZdcReference.Features.Nasr.ScheduledJobs;

public class FetchNasrData(NasrDataService nasrDataService) : IInvocable
{
    public async Task Invoke()
    {
        await nasrDataService.FetchAndCacheData();
    }
}
