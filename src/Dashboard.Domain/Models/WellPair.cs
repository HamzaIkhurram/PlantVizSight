// WellPair.cs - Represents an injector-producer well pair in SAGD operations
namespace Dashboard.Domain.Models;

public class WellPair
{
    public Guid WellPairId { get; set; }
    public required string WellPairName { get; set; }
    public required string Facility { get; set; }
    public required string Pad { get; set; }
    public double SubcoolTemperature { get; set; }
    public double WellheadPressure { get; set; }
    public double ProductionRate { get; set; }
    public double SteamInjection { get; set; }
    public double SorRatio { get; set; }
    public double SystemUptime { get; set; }
    public DateTime LastUpdate { get; set; }
    public string Status { get; set; } = "Normal";
}

