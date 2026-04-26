using System.Collections.Generic;

namespace ZdcReference.Features.PirepEncoder.Models;

public sealed record Weather
{
    public int? FlightVisibilitySm { get; init; }

    public IReadOnlyList<string> Contractions { get; init; } = new List<string>();
}
