namespace ZdcReference.Features.Nasr.Models;

public record AirwayFix(
    string FixId,
    string AirwayId,
    int Sequence,
    double Latitude,
    double Longitude);
