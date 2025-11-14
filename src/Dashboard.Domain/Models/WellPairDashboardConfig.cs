// WellPairDashboardConfig.cs - Configuration for well pair dashboard visualization settings
namespace Dashboard.Domain.Models;

public class WellPairDashboardConfig
{
    public double SubcoolOptimalMin { get; set; } = 175.0;
    public double SubcoolOptimalMax { get; set; } = 205.0;
    public double SubcoolChartMin { get; set; } = 160.0;
    public double SubcoolChartMax { get; set; } = 220.0;
    public double SorChartMin { get; set; } = 3.0;
    public double SorChartMax { get; set; } = 3.5;
    public double ProductionChartMin { get; set; } = 300.0;
    public double TemperatureChartMin { get; set; } = 180.0;
    public double TemperatureChartMax { get; set; } = 195.0;
    public int TopPairsCount { get; set; } = 10;
}

