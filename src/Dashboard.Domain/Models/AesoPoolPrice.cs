// AesoPoolPrice.cs - AESO Pool Price Report models
namespace Dashboard.Domain.Models;

public class AesoPoolPriceReport
{
    public List<AesoPoolPriceEntry> PriceData { get; set; } = new();
}

public class AesoPoolPriceEntry
{
    public string BeginDatetimeUtc { get; set; } = string.Empty;
    public string BeginDatetimeMpt { get; set; } = string.Empty;
    public string PoolPrice { get; set; } = string.Empty;
    public string ForecastPoolPrice { get; set; } = string.Empty;
    public string Rolling30DayAvg { get; set; } = string.Empty;
    public bool IsSettled => !string.IsNullOrEmpty(PoolPrice);
    
    public decimal? PoolPriceDecimal
    {
        get
        {
            if (string.IsNullOrEmpty(PoolPrice))
                return null;
            return decimal.TryParse(PoolPrice, out var price) ? price : null;
        }
    }
    
    public decimal? ForecastPriceDecimal
    {
        get
        {
            if (string.IsNullOrEmpty(ForecastPoolPrice))
                return null;
            return decimal.TryParse(ForecastPoolPrice, out var price) ? price : null;
        }
    }
    
    public decimal? Rolling30DayAvgDecimal
    {
        get
        {
            if (string.IsNullOrEmpty(Rolling30DayAvg))
                return null;
            return decimal.TryParse(Rolling30DayAvg, out var price) ? price : null;
        }
    }
}

