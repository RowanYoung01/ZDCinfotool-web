using System.Text.Json;
using Coravel.Invocable;
using ZdcReference.Features.Scratchpads.Models;
using ZdcReference.Features.Scratchpads.Repositories;

namespace ZdcReference.Features.Scratchpads.ScheduledJobs;


public class FetchAndStoreScratchpads(
    ILogger<FetchAndStoreScratchpads> logger,
    IWebHostEnvironment env,
    ScratchpadsRepository scratchpadsRepository)
    : IInvocable
{
    public async Task Invoke()
    {
        var scratchpadsPath = Path.Combine(env.WebRootPath, "data", "v1", "scratchpads.json");
        try
        {
            logger.LogInformation("Starting scratchpad fetch and update task");
            await using var fileStream = File.OpenRead(scratchpadsPath);
            var scratchpads = await JsonSerializer.DeserializeAsync<List<AirportScratchpad>>(fileStream);

            if (scratchpads is null)
            {
                logger.LogWarning("Error while reading scratchpads: null JSON deserialization from {path}", scratchpadsPath);
                return;
            }

            logger.LogInformation("Successfully read scratchpads from {path}", scratchpadsPath);

            scratchpadsRepository.ClearAirports();
            logger.LogInformation("Deleted all scratchpads");

            var count = 0;
            foreach (var airport in scratchpads)
            {
                if (!scratchpadsRepository.TryAddScratchpads(airport.Id, airport.Scratchpads))
                {
                    logger.LogWarning("Error adding {id} to Scratchpad Repository", airport.Id);
                    continue;
                }

                count += 1;
            }
            
            logger.LogInformation("Added {num} airport scratchpad definitions to Scratchpad Repository", count);
        }
        catch (Exception e)
        {
            logger.LogWarning("Exception while trying to fetch and update scratchpads: {ex}", e);
        }
    }
}
