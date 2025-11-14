// WellPairMessage.cs - Real-time well pair data DTO for SignalR distribution
namespace Dashboard.Contracts.DTOs;

public record WellPairMessage
{
    public required string WellPairName { get; init; }
    public required string Facility { get; init; }
    public required string Pad { get; init; }
    public double SubcoolTemperature { get; init; }
    public double WellheadPressure { get; init; }
    public double ProductionRate { get; init; }
    public double SteamInjection { get; init; }
    public double SorRatio { get; init; }
    public double SystemUptime { get; init; }
    public required string Status { get; init; }
    public DateTime Timestamp { get; init; }
}

