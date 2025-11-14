// WellPairKpiMessage.cs - Well pair KPI summary DTO for SignalR distribution
namespace Dashboard.Contracts.DTOs;

public record WellPairKpiMessage
{
    public double TotalBitumenProduction { get; init; }
    public double TotalSteamInjection { get; init; }
    public double AvgSubcoolTemperature { get; init; }
    public double SystemUptime { get; init; }
    public double AvgSor { get; init; }
    public int ActiveWellPairs { get; init; }
    public DateTime Timestamp { get; init; }
}

