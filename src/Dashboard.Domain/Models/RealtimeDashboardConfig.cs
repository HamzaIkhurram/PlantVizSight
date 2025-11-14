// RealtimeDashboardConfig.cs - Configuration for Real-Time Monitoring Dashboard
namespace Dashboard.Domain.Models;

public class RealtimeDashboardConfig
{
    public int MaxDataPoints { get; set; } = 20;
    public string TemperatureLabel { get; set; } = "Temperature (°C)";
    public string PressureLabel { get; set; } = "Pressure (PSI)";
    public string TemperatureColor { get; set; } = "#dc3545";
    public string TemperatureBgColor { get; set; } = "rgba(220, 53, 69, 0.1)";
    public string PressureColor { get; set; } = "#17a2b8";
    public string PressureBgColor { get; set; } = "rgba(23, 162, 184, 0.1)";
    public double ChartTension { get; set; } = 0.4;
    public int ChartBorderWidth { get; set; } = 2;
    public bool ChartBeginAtZero { get; set; } = false;
    public int ValueDecimalPlaces { get; set; } = 2;
    public int RetryDelayMs { get; set; } = 5000;
    public string DefaultUnit { get; set; } = "°C / PSI";
    public string DefaultStatus { get; set; } = "Good";
    public int AnimationDelayMs { get; set; } = 300;
    public string[] TemperatureKeywords { get; set; } = new[] { "Temp", "Temperature" };
    public string[] PressureKeywords { get; set; } = new[] { "Press", "Pressure" };
}

