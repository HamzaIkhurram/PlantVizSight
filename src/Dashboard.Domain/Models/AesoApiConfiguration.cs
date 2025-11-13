// AesoApiConfiguration.cs - Configuration for AESO API integration
namespace Dashboard.Domain.Models;

public class AesoApiConfiguration
{
    public string BaseUrl { get; set; } = "https://apimgw.aeso.ca/public/currentsupplydemand-api/v1";
    public string? SubscriptionKey { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 2;
}

