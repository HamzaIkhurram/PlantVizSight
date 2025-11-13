// AesoAssetGeneration.cs - AESO Current Supply Demand Asset Generation Report models
namespace Dashboard.Domain.Models;

public class AesoAssetGenerationReport
{
    public string? LastUpdatedDatetimeUtc { get; set; }
    public string? LastUpdatedDatetimeMpt { get; set; }
    public List<AesoAsset> AssetList { get; set; } = new();
}

public class AesoAsset
{
    public string Asset { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public string? SubFuelType { get; set; }
    public int MaximumCapability { get; set; }
    public int NetGeneration { get; set; }
    public int DispatchedContingencyReserve { get; set; }
}

