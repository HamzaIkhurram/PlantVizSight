// SorDashboardConfig.cs - Configuration for SAGD SOR dashboard visualization settings
namespace Dashboard.Domain.Models;

public class SorDashboardConfig
{
    public double SorOptimalMin { get; set; } = 2.0;
    public double SorOptimalMax { get; set; } = 3.5;
    public double SorInefficientMin { get; set; } = 4.0;
    public double SorInefficientMax { get; set; } = 5.0;
    public double SorCriticalMin { get; set; } = 5.0;
    public double SorCriticalMax { get; set; } = 8.0;
    public double SorChartMin { get; set; } = 1.0;
    public double SorChartMax { get; set; } = 8.0;
    public double SteamChartMin { get; set; } = 5000.0;
    public double SteamChartMax { get; set; } = 8000.0;
    public double BitumenChartMin { get; set; } = 1500.0;
    public double BitumenChartMax { get; set; } = 2800.0;
    public double SteamQualityChartMin { get; set; } = 16000.0;
    public double SteamQualityChartMax { get; set; } = 18000.0;
    public double ReservoirTempChartMin { get; set; } = 200.0;
    public double ReservoirTempChartMax { get; set; } = 230.0;
    public double EspStatusChartMin { get; set; } = 70.0;
    public double EspStatusChartMax { get; set; } = 100.0;
    public int MaxDataPoints { get; set; } = 60;
}

