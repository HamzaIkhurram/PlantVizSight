// WellPairKpi.cs - Key performance indicators for well pair operations
namespace Dashboard.Domain.Models;

public class WellPairKpi
{
    public double TotalBitumenProduction { get; set; }
    public double TotalSteamInjection { get; set; }
    public double AvgSubcoolTemperature { get; set; }
    public double SystemUptime { get; set; }
    public double AvgSor { get; set; }
    public int ActiveWellPairs { get; set; }
}

