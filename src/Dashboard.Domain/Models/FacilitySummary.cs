// FacilitySummary.cs - Summary statistics for SAGD facilities
namespace Dashboard.Domain.Models;

public class FacilitySummary
{
    public required string FacilityName { get; set; }
    public double AvgProduction { get; set; }
    public double AvgSor { get; set; }
    public double PressureControl { get; set; }
    public double Uptime { get; set; }
    public int WellPairCount { get; set; }
}

