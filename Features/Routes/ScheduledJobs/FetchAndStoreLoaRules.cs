using System.Globalization;
using System.Text.RegularExpressions;
using Coravel.Invocable;
using CsvHelper;
using CsvHelper.Configuration;
using ZdcReference.Features.Routes.Models;
using ZdcReference.Features.Routes.Repositories;

namespace ZdcReference.Features.Routes.ScheduledJobs;

public class FetchAndStoreLoaRules(ILogger<FetchAndStoreLoaRules> logger, LoaRuleRepository loaRules, IWebHostEnvironment env) : IInvocable
{
    public Task Invoke()
    {
        var loaPath = Path.Combine(env.WebRootPath, "data", "v1", "loa.csv");
        using var responseStream = File.OpenRead(loaPath);
        using var reader = new StreamReader(responseStream);
        logger.LogInformation("Read LOA file from: {path}", loaPath);
        
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<LoaRuleMap>();
        var records = csv.GetRecords<LoaRule>().ToList();
        
        loaRules.ClearRules();
        loaRules.AddRules(records);
        return Task.CompletedTask;
    }

    private class LoaRuleMap : ClassMap<LoaRule>
    {
        public LoaRuleMap()
        {
            Map(m => m.DepartureAirportRegex).Convert(args => new Regex(args.Row.GetField("Departure_Regex"), RegexOptions.IgnoreCase));
            Map(m => m.ArrivalAirportRegex).Convert(args => new Regex(args.Row.GetField("Arrival_Regex"), RegexOptions.IgnoreCase));
            Map(m => m.Route).Name("Route");
            Map(m => m.IsRnavRequired).Name("RNAV Required");
            Map(m => m.Notes).Name("Notes");
        }
    }
}