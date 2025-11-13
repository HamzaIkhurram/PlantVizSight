using Dashboard.Domain.Models;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dashboard.Domain.Services;

/// <summary>
/// Service for fetching live data from AESO Current Supply Demand API
/// FAANG-level implementation with retry logic, proper error handling, and resilience
/// </summary>
public class AesoApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AesoApiService> _logger;
    private readonly AesoApiConfiguration _config;

    public AesoApiService(
        HttpClient httpClient, 
        ILogger<AesoApiService> logger,
        IOptions<AesoApiConfiguration> config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config?.Value ?? new AesoApiConfiguration();

        // Configure HttpClient timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
        
        _logger.LogInformation("AesoApiService initialized. API Key configured: {HasKey}", 
            !string.IsNullOrEmpty(_config.SubscriptionKey));
    }

    /// <summary>
    /// Fetch current generation data for all AIES assets greater than 5 MW
    /// FAANG-level: Implements retry logic, proper error handling, and detailed diagnostics
    /// </summary>
    /// <param name="assetIds">Optional comma-separated asset IDs (up to 20)</param>
    /// <returns>AESO Asset Generation Report</returns>
    public async Task<AesoAssetGenerationReport?> GetCurrentSupplyDemandAsync(string? assetIds = null)
    {
        try
        {
            // Check if subscription key is configured
            if (string.IsNullOrEmpty(_config.SubscriptionKey))
            {
                _logger.LogError("AESO API Subscription Key is not configured. Please add it to appsettings.json");
                throw new InvalidOperationException(
                    "AESO API Subscription Key is missing. " +
                    "Get your key from https://api.aeso.ca/ and add it to AesoApi:SubscriptionKey in appsettings.json");
            }

            var url = $"{_config.BaseUrl}/csd/generation/assets/current";
            if (!string.IsNullOrEmpty(assetIds))
            {
                url += $"?assetIds={Uri.EscapeDataString(assetIds)}";
            }

            _logger.LogInformation("Fetching AESO data from: {Url}", url);

            // Execute request with manual retry logic
            HttpResponseMessage? response = null;
            Exception? lastException = null;

            for (int attempt = 1; attempt <= _config.RetryAttempts; attempt++)
            {
                try
                {
                    // Create request with subscription key header
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("API-KEY", _config.SubscriptionKey);
                    request.Headers.Add("Cache-Control", "no-cache");
                    request.Headers.Add("Accept", "application/json");
                    request.Headers.Add("User-Agent", "PlantSight-Dashboard/1.0");

                    response = await _httpClient.SendAsync(request);
                    
                    // Enhanced error logging
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError(
                            "AESO API returned {StatusCode}: {ErrorContent}",
                            response.StatusCode, errorContent);
                        
                        // Retry on server errors or rate limiting
                        if ((int)response.StatusCode >= 500 || response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            if (attempt < _config.RetryAttempts)
                            {
                                var delay = TimeSpan.FromSeconds(Math.Pow(_config.RetryDelaySeconds, attempt));
                                _logger.LogWarning("Retrying after {Delay}ms... (Attempt {Attempt}/{Total})", 
                                    delay.TotalMilliseconds, attempt + 1, _config.RetryAttempts);
                                await Task.Delay(delay);
                                continue;
                            }
                        }
                    }
                    
                    response.EnsureSuccessStatusCode();
                    break; // Success!
                }
                catch (HttpRequestException ex) when (attempt < _config.RetryAttempts)
                {
                    lastException = ex;
                    var delay = TimeSpan.FromSeconds(Math.Pow(_config.RetryDelaySeconds, attempt));
                    _logger.LogWarning(ex, "Request failed. Retrying after {Delay}ms... (Attempt {Attempt}/{Total})", 
                        delay.TotalMilliseconds, attempt + 1, _config.RetryAttempts);
                    await Task.Delay(delay);
                }
            }

            if (response == null || !response.IsSuccessStatusCode)
            {
                throw lastException ?? new HttpRequestException("Failed to fetch data from AESO API");
            }

            var wrapper = await response.Content.ReadFromJsonAsync<AesoApiWrapper>();
            
            if (wrapper?.@return == null)
            {
                _logger.LogWarning("AESO API returned null data");
                return null;
            }

            var data = wrapper.@return;

            // Map API response to domain model
            var report = new AesoAssetGenerationReport
            {
                LastUpdatedDatetimeUtc = data.last_updated_datetime_utc,
                LastUpdatedDatetimeMpt = data.last_updated_datetime_mpt,
                AssetList = data.asset_list?.Select(a => new AesoAsset
                {
                    Asset = a.asset ?? string.Empty,
                    FuelType = a.fuel_type ?? string.Empty,
                    SubFuelType = a.sub_fuel_type,
                    MaximumCapability = a.maximum_capability,
                    NetGeneration = a.net_generation,
                    DispatchedContingencyReserve = a.dispatched_contingency_reserve
                }).ToList() ?? new List<AesoAsset>()
            };

            _logger.LogInformation("✅ Successfully fetched {Count} AESO assets", report.AssetList.Count);
            return report;
        }
        catch (InvalidOperationException)
        {
            // Re-throw configuration errors
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "❌ HTTP error fetching AESO data. Check network connectivity and API status.");
            throw new Exception("Failed to connect to AESO API. Please check your internet connection and try again.", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "⏱️ AESO API request timed out after {Timeout} seconds", _config.TimeoutSeconds);
            throw new Exception($"AESO API request timed out after {_config.TimeoutSeconds} seconds.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Unexpected error fetching AESO data");
            throw new Exception("An unexpected error occurred while fetching AESO data. Please try again later.", ex);
        }
    }

    /// <summary>
    /// Get assets filtered by fuel type
    /// </summary>
    public async Task<List<AesoAsset>> GetAssetsByFuelTypeAsync(string fuelType)
    {
        var report = await GetCurrentSupplyDemandAsync();
        if (report == null) return new List<AesoAsset>();

        return report.AssetList
            .Where(a => a.FuelType.Equals(fuelType, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Get total generation by fuel type
    /// </summary>
    public async Task<Dictionary<string, int>> GetGenerationByFuelTypeAsync()
    {
        var report = await GetCurrentSupplyDemandAsync();
        if (report == null) return new Dictionary<string, int>();

        return report.AssetList
            .GroupBy(a => a.FuelType)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(a => a.NetGeneration)
            );
    }

    /// <summary>
    /// Get current supply demand summary from v2 API
    /// </summary>
    public async Task<AesoV2SummaryReport?> GetCurrentSummaryV2Async()
    {
        try
        {
            // Check if subscription key is configured
            if (string.IsNullOrEmpty(_config.SubscriptionKey))
            {
                _logger.LogError("AESO API Subscription Key is not configured");
                throw new InvalidOperationException(
                    "AESO API Subscription Key is missing. " +
                    "Get your key from https://api.aeso.ca/ and add it to AesoApi:SubscriptionKey in appsettings.json");
            }

            var url = $"{_config.BaseUrl.Replace("/v1", "/v2")}/csd/summary/current";

            _logger.LogInformation("Fetching AESO v2 summary from: {Url}", url);

            // Execute request with manual retry logic
            HttpResponseMessage? response = null;
            Exception? lastException = null;

            for (int attempt = 1; attempt <= _config.RetryAttempts; attempt++)
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("API-KEY", _config.SubscriptionKey);
                    request.Headers.Add("Cache-Control", "no-cache");
                    request.Headers.Add("Accept", "application/json");
                    request.Headers.Add("User-Agent", "PlantSight-Dashboard/1.0");

                    response = await _httpClient.SendAsync(request);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError("AESO v2 API returned {StatusCode}: {ErrorContent}", 
                            response.StatusCode, errorContent);
                        
                        if ((int)response.StatusCode >= 500 || response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            if (attempt < _config.RetryAttempts)
                            {
                                var delay = TimeSpan.FromSeconds(Math.Pow(_config.RetryDelaySeconds, attempt));
                                _logger.LogWarning("Retrying after {Delay}ms... (Attempt {Attempt}/{Total})", 
                                    delay.TotalMilliseconds, attempt + 1, _config.RetryAttempts);
                                await Task.Delay(delay);
                                continue;
                            }
                        }
                    }
                    
                    response.EnsureSuccessStatusCode();
                    break;
                }
                catch (HttpRequestException ex) when (attempt < _config.RetryAttempts)
                {
                    lastException = ex;
                    var delay = TimeSpan.FromSeconds(Math.Pow(_config.RetryDelaySeconds, attempt));
                    _logger.LogWarning(ex, "Request failed. Retrying after {Delay}ms...", delay.TotalMilliseconds);
                    await Task.Delay(delay);
                }
            }

            if (response == null || !response.IsSuccessStatusCode)
            {
                throw lastException ?? new HttpRequestException("Failed to fetch v2 summary from AESO API");
            }

            var wrapper = await response.Content.ReadFromJsonAsync<AesoV2Wrapper>();
            
            if (wrapper?.@return == null)
            {
                _logger.LogWarning("AESO v2 API returned null data");
                return null;
            }

            var data = wrapper.@return;

            // Map API response to domain model
            var report = new AesoV2SummaryReport
            {
                LastUpdatedDatetimeUtc = data.effective_datetime_utc,
                LastUpdatedDatetimeMpt = data.effective_datetime_mpt,
                TotalMaxGeneration = data.total_max_generation_capability,
                TotalNetGeneration = data.total_net_generation,
                NetToGridGeneration = data.net_to_grid_generation,
                DispatchedContingencyReserve = data.dispatched_contigency_reserve_total, // Note: API has typo
                LssoPriceSettingDispatchedGeneration = data.lsso_price_setting_dispatched_generation,
                AlbertaInternalLoad = data.alberta_internal_load,
                ContingentEvents = data.contingent_events,
                FuelTypeDetails = data.generation_data_list?.Select(f => new AesoV2FuelTypeDetail
                {
                    FuelType = f.fuel_type ?? string.Empty,
                    MaxGeneration = f.aggregated_maximum_capability,
                    NetGeneration = f.aggregated_net_generation,
                    DispatchedContingencyReserve = f.aggregated_dispatched_contingency_reserve
                }).ToList() ?? new List<AesoV2FuelTypeDetail>()
            };

            _logger.LogInformation("✅ Successfully fetched AESO v2 summary");
            return report;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "❌ HTTP error fetching AESO v2 summary");
            throw new Exception("Failed to connect to AESO v2 API. Please check your internet connection and try again.", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "⏱️ AESO v2 API request timed out");
            throw new Exception($"AESO v2 API request timed out after {_config.TimeoutSeconds} seconds.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Unexpected error fetching AESO v2 summary");
            throw new Exception("An unexpected error occurred while fetching AESO v2 summary.", ex);
        }
    }

    /// <summary>
    /// Get pool price data for a date range
    /// </summary>
    /// <param name="startDate">Start date (yyyy-MM-dd), between 2000-01-01 and current date</param>
    /// <param name="endDate">Optional end date (yyyy-MM-dd). If omitted, only startDate data is returned. Maximum 1 year range.</param>
    /// <returns>Pool price report with hourly data</returns>
    public async Task<AesoPoolPriceReport?> GetPoolPriceAsync(string startDate, string? endDate = null)
    {
        try
        {
            // Validate subscription key
            if (string.IsNullOrEmpty(_config.SubscriptionKey))
            {
                _logger.LogError("AESO API Subscription Key is not configured");
                throw new InvalidOperationException(
                    "AESO API Subscription Key is missing. " +
                    "Get your key from https://api.aeso.ca/ and add it to AesoApi:SubscriptionKey in appsettings.json");
            }

            // Validate date format
            if (!DateTime.TryParse(startDate, out var startDateParsed))
            {
                throw new ArgumentException($"Invalid startDate format: {startDate}. Expected yyyy-MM-dd");
            }

            if (startDateParsed < new DateTime(2000, 1, 1) || startDateParsed > DateTime.Today)
            {
                throw new ArgumentException($"startDate must be between 2000-01-01 and today's date");
            }

            // Build URL - Pool Price API is v1.1
            var url = "https://apimgw.aeso.ca/public/poolprice-api/v1.1/price/poolPrice?startDate=" + startDate;
            
            if (!string.IsNullOrEmpty(endDate))
            {
                if (!DateTime.TryParse(endDate, out var endDateParsed))
                {
                    throw new ArgumentException($"Invalid endDate format: {endDate}. Expected yyyy-MM-dd");
                }

                if (endDateParsed < startDateParsed)
                {
                    throw new ArgumentException("endDate must be after startDate");
                }

                if ((endDateParsed - startDateParsed).TotalDays > 365)
                {
                    throw new ArgumentException("Date range cannot exceed 1 year");
                }

                url += $"&endDate={endDate}";
            }

            _logger.LogInformation("Fetching AESO Pool Price from: {Url}", url);

            // Execute request with manual retry logic
            HttpResponseMessage? response = null;
            Exception? lastException = null;

            for (int attempt = 1; attempt <= _config.RetryAttempts; attempt++)
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("API-KEY", _config.SubscriptionKey);
                    request.Headers.Add("Cache-Control", "no-cache");
                    request.Headers.Add("Accept", "application/json");
                    request.Headers.Add("User-Agent", "PlantSight-Dashboard/1.0");

                    response = await _httpClient.SendAsync(request);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError("AESO Pool Price API returned {StatusCode}: {ErrorContent}", 
                            response.StatusCode, errorContent);
                        
                        if ((int)response.StatusCode >= 500 || response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            if (attempt < _config.RetryAttempts)
                            {
                                var delay = TimeSpan.FromSeconds(Math.Pow(_config.RetryDelaySeconds, attempt));
                                _logger.LogWarning("Retrying after {Delay}ms... (Attempt {Attempt}/{Total})", 
                                    delay.TotalMilliseconds, attempt + 1, _config.RetryAttempts);
                                await Task.Delay(delay);
                                continue;
                            }
                        }
                    }
                    
                    response.EnsureSuccessStatusCode();
                    break;
                }
                catch (HttpRequestException ex) when (attempt < _config.RetryAttempts)
                {
                    lastException = ex;
                    var delay = TimeSpan.FromSeconds(Math.Pow(_config.RetryDelaySeconds, attempt));
                    _logger.LogWarning(ex, "Request failed. Retrying after {Delay}ms...", delay.TotalMilliseconds);
                    await Task.Delay(delay);
                }
            }

            if (response == null || !response.IsSuccessStatusCode)
            {
                throw lastException ?? new HttpRequestException("Failed to fetch pool price from AESO API");
            }

            var wrapper = await response.Content.ReadFromJsonAsync<AesoPoolPriceWrapper>();
            
            if (wrapper?.@return?.PoolPriceReport == null)
            {
                _logger.LogWarning("AESO Pool Price API returned null data");
                return null;
            }

            var data = wrapper.@return.PoolPriceReport;

            // Map to domain model
            var report = new AesoPoolPriceReport
            {
                PriceData = data.Select(p => new AesoPoolPriceEntry
                {
                    BeginDatetimeUtc = p.begin_datetime_utc ?? string.Empty,
                    BeginDatetimeMpt = p.begin_datetime_mpt ?? string.Empty,
                    PoolPrice = p.pool_price ?? string.Empty,
                    ForecastPoolPrice = p.forecast_pool_price ?? string.Empty,
                    Rolling30DayAvg = p.rolling_30day_avg ?? string.Empty
                }).ToList()
            };

            _logger.LogInformation("✅ Successfully fetched {Count} pool price records", report.PriceData.Count);
            return report;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "❌ HTTP error fetching pool price data");
            throw new Exception("Failed to connect to AESO Pool Price API. Please check your internet connection.", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "⏱️ Pool Price API request timed out");
            throw new Exception($"Pool Price API request timed out after {_config.TimeoutSeconds} seconds.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Unexpected error fetching pool price data");
            throw new Exception("An unexpected error occurred while fetching pool price data.", ex);
        }
    }
}

/// <summary>
/// Wrapper for AESO API response
/// </summary>
internal class AesoApiWrapper
{
    public string? timestamp { get; set; }
    public string? responseCode { get; set; }
    public AesoApiResponse? @return { get; set; }
}

/// <summary>
/// Internal DTO for mapping AESO API JSON response (snake_case)
/// </summary>
internal class AesoApiResponse
{
    public string? last_updated_datetime_utc { get; set; }
    public string? last_updated_datetime_mpt { get; set; }
    public List<AesoApiAsset>? asset_list { get; set; }
}

internal class AesoApiAsset
{
    public string? asset { get; set; }
    public string? fuel_type { get; set; }
    public string? sub_fuel_type { get; set; }
    public int maximum_capability { get; set; }
    public int net_generation { get; set; }
    public int dispatched_contingency_reserve { get; set; }
}

/// <summary>
/// Wrapper for AESO v2 API response
/// </summary>
internal class AesoV2Wrapper
{
    public string? timestamp { get; set; }
    public string? responseCode { get; set; }
    public AesoV2Response? @return { get; set; }
}

/// <summary>
/// Internal DTO for v2 summary response
/// </summary>
internal class AesoV2Response
{
    public string? effective_datetime_utc { get; set; }
    public string? effective_datetime_mpt { get; set; }
    public int total_max_generation_capability { get; set; }
    public int total_net_generation { get; set; }
    public int net_to_grid_generation { get; set; }
    public int dispatched_contigency_reserve_total { get; set; } // Note: API has typo "contigency"
    public int lsso_price_setting_dispatched_generation { get; set; }
    public int alberta_internal_load { get; set; }
    public int contingent_events { get; set; }
    public List<AesoV2FuelType>? generation_data_list { get; set; }
}

internal class AesoV2FuelType
{
    public string? fuel_type { get; set; }
    public int aggregated_maximum_capability { get; set; }
    public int aggregated_net_generation { get; set; }
    public int aggregated_dispatched_contingency_reserve { get; set; }
}

/// <summary>
/// Wrapper for Pool Price API response
/// </summary>
internal class AesoPoolPriceWrapper
{
    public string? timestamp { get; set; }
    public string? responseCode { get; set; }
    public AesoPoolPriceResponse? @return { get; set; }
}

/// <summary>
/// Internal DTO for Pool Price response
/// Note: The property name in the API is "Pool Price Report" with spaces!
/// </summary>
internal class AesoPoolPriceResponse
{
    [JsonPropertyName("Pool Price Report")]
    public List<AesoPoolPriceData>? PoolPriceReport { get; set; }
}

/// <summary>
/// Internal DTO for individual pool price data point
/// </summary>
internal class AesoPoolPriceData
{
    public string? begin_datetime_utc { get; set; }
    public string? begin_datetime_mpt { get; set; }
    public string? pool_price { get; set; }
    public string? forecast_pool_price { get; set; }
    public string? rolling_30day_avg { get; set; }
}

