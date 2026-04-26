using System.Text;
using ZdcReference.Features.PirepEncoder.Models;

namespace ZdcReference.Features.PirepEncoder.Services;

public static class WeatherFormatter
{
    public static string Format(Weather weather)
    {
        var sb = new StringBuilder();
        if (weather.FlightVisibilitySm is int fv)
        {
            sb.Append("FV").Append(fv.ToString("D2", System.Globalization.CultureInfo.InvariantCulture));
        }
        foreach (var c in weather.Contractions)
        {
            if (string.IsNullOrWhiteSpace(c))
            {
                continue;
            }
            if (sb.Length > 0)
            {
                sb.Append(' ');
            }
            sb.Append(c);
        }
        return sb.ToString();
    }
}
