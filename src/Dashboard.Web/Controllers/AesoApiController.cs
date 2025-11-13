// AesoApiController.cs - REST API controller for AESO Current Supply Demand and Pool Price APIs
using Microsoft.AspNetCore.Mvc;
using Dashboard.Domain.Services;

namespace Dashboard.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AesoApiController : ControllerBase
{
    private readonly AesoApiService _aesoService;
    private readonly ILogger<AesoApiController> _logger;

    public AesoApiController(AesoApiService aesoService, ILogger<AesoApiController> logger)
    {
        _aesoService = aesoService;
        _logger = logger;
        _logger.LogInformation("AesoApiController initialized");
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { 
            status = "healthy",
            message = "AESO API Controller is running",
            timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentSupplyDemand([FromQuery] string? assetIds = null)
    {
        try
        {
            var data = await _aesoService.GetCurrentSupplyDemandAsync(assetIds);
            
            if (data == null)
            {
                return NotFound(new { 
                    error = "No data available from AESO API",
                    message = "The AESO API did not return any data. Please try again later."
                });
            }

            return Ok(data);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Configuration error");
            return BadRequest(new { 
                error = "Configuration Error",
                message = ex.Message,
                details = "Please configure your AESO API subscription key in appsettings.json"
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch AESO data");
            return StatusCode(503, new { 
                error = "Service Unavailable",
                message = "Unable to connect to AESO API. Please check your internet connection and try again.",
                details = ex.Message
            });
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout");
            return StatusCode(408, new { 
                error = "Request Timeout",
                message = "The request to AESO API timed out. Please try again.",
                details = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AESO data request");
            return StatusCode(500, new { 
                error = "Internal Server Error",
                message = "An unexpected error occurred. Please try again later.",
                details = ex.Message
            });
        }
    }

    [HttpGet("fuel-type/{fuelType}")]
    public async Task<IActionResult> GetAssetsByFuelType(string fuelType)
    {
        try
        {
            var assets = await _aesoService.GetAssetsByFuelTypeAsync(fuelType);
            return Ok(assets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching assets by fuel type: {FuelType}", fuelType);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("generation-by-fuel")]
    public async Task<IActionResult> GetGenerationByFuelType()
    {
        try
        {
            var data = await _aesoService.GetGenerationByFuelTypeAsync();
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching generation by fuel type");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        try
        {
            _logger.LogInformation("GetSummary called");
            var report = await _aesoService.GetCurrentSupplyDemandAsync();
            
            if (report == null)
            {
                return NotFound(new { 
                    error = "No data available",
                    message = "The AESO API did not return any data."
                });
            }

            var summary = new
            {
                LastUpdated = report.LastUpdatedDatetimeMpt,
                TotalAssets = report.AssetList.Count,
                TotalGeneration = report.AssetList.Sum(a => a.NetGeneration),
                TotalCapacity = report.AssetList.Sum(a => a.MaximumCapability),
                TotalReserve = report.AssetList.Sum(a => a.DispatchedContingencyReserve),
                CapacityUtilization = report.AssetList.Sum(a => a.MaximumCapability) > 0
                    ? (double)report.AssetList.Sum(a => a.NetGeneration) / report.AssetList.Sum(a => a.MaximumCapability) * 100
                    : 0,
                FuelTypes = report.AssetList.GroupBy(a => a.FuelType).Select(g => new
                {
                    FuelType = g.Key,
                    Count = g.Count(),
                    Generation = g.Sum(a => a.NetGeneration),
                    Capacity = g.Sum(a => a.MaximumCapability)
                }).OrderByDescending(x => x.Generation).ToList()
            };

            return Ok(summary);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Configuration error");
            return BadRequest(new { 
                error = "Configuration Error",
                message = ex.Message,
                details = "Please configure your AESO API subscription key in appsettings.json"
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch AESO data");
            return StatusCode(503, new { 
                error = "Service Unavailable",
                message = "Unable to connect to AESO API.",
                details = ex.Message
            });
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout");
            return StatusCode(408, new { 
                error = "Request Timeout",
                message = "The request to AESO API timed out.",
                details = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching AESO summary");
            return StatusCode(500, new { 
                error = "Internal Server Error",
                message = "An unexpected error occurred: " + ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    [HttpGet("v2/summary")]
    public async Task<IActionResult> GetV2Summary()
    {
        try
        {
            _logger.LogInformation("GetV2Summary called");
            var summary = await _aesoService.GetCurrentSummaryV2Async();
            
            if (summary == null)
            {
                return NotFound(new { 
                    error = "No data available",
                    message = "The AESO v2 API did not return any data."
                });
            }

            return Ok(summary);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Configuration error");
            return BadRequest(new { 
                error = "Configuration Error",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching AESO v2 summary");
            return StatusCode(500, new { 
                error = "Internal Server Error",
                message = "An unexpected error occurred: " + ex.Message
            });
        }
    }

    [HttpGet("poolprice")]
    public async Task<IActionResult> GetPoolPrice([FromQuery] string startDate, [FromQuery] string? endDate = null)
    {
        try
        {
            _logger.LogInformation("GetPoolPrice called with startDate={StartDate}, endDate={EndDate}", startDate, endDate);
            
            var data = await _aesoService.GetPoolPriceAsync(startDate, endDate);
            
            if (data == null)
            {
                return NotFound(new { 
                    error = "No data available",
                    message = "The AESO Pool Price API did not return any data for the specified date range."
                });
            }

            return Ok(data);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for pool price request");
            return BadRequest(new { 
                error = "Invalid Request",
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Configuration error");
            return BadRequest(new { 
                error = "Configuration Error",
                message = ex.Message
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch pool price data");
            return StatusCode(503, new { 
                error = "Service Unavailable",
                message = "Unable to connect to AESO Pool Price API.",
                details = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pool price data");
            return StatusCode(500, new { 
                error = "Internal Server Error",
                message = "An unexpected error occurred: " + ex.Message
            });
        }
    }

    [HttpGet("poolprice/summary")]
    public async Task<IActionResult> GetPoolPriceSummary([FromQuery] string startDate, [FromQuery] string? endDate = null)
    {
        try
        {
            _logger.LogInformation("GetPoolPriceSummary called");
            var data = await _aesoService.GetPoolPriceAsync(startDate, endDate);
            
            if (data == null || data.PriceData.Count == 0)
            {
                return NotFound(new { 
                    error = "No data available",
                    message = "No pool price data found for the specified date range."
                });
            }

            var settledPrices = data.PriceData
                .Where(p => p.IsSettled && p.PoolPriceDecimal.HasValue)
                .Select(p => p.PoolPriceDecimal!.Value)
                .ToList();

            if (settledPrices.Count == 0)
            {
                return Ok(new
                {
                    Message = "No settled prices available yet",
                    TotalHours = data.PriceData.Count,
                    SettledHours = 0
                });
            }

            var summary = new
            {
                StartDate = startDate,
                EndDate = endDate ?? startDate,
                TotalHours = data.PriceData.Count,
                SettledHours = settledPrices.Count,
                AveragePrice = Math.Round(settledPrices.Average(), 2),
                MinPrice = Math.Round(settledPrices.Min(), 2),
                MaxPrice = Math.Round(settledPrices.Max(), 2),
                MedianPrice = Math.Round(GetMedian(settledPrices), 2),
                Rolling30DayAvg = data.PriceData.FirstOrDefault(p => !string.IsNullOrEmpty(p.Rolling30DayAvg))?.Rolling30DayAvgDecimal
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating pool price summary");
            return StatusCode(500, new { 
                error = "Internal Server Error",
                message = ex.Message
            });
        }
    }

    private static decimal GetMedian(List<decimal> values)
    {
        var sorted = values.OrderBy(x => x).ToList();
        int mid = sorted.Count / 2;
        return sorted.Count % 2 == 0 
            ? (sorted[mid - 1] + sorted[mid]) / 2 
            : sorted[mid];
    }
}

