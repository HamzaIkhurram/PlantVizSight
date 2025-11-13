// AesoV2Summary.cs - AESO API v2 Current Supply Demand Summary models
namespace Dashboard.Domain.Models;

public class AesoV2SummaryReport
{
    public string? LastUpdatedDatetimeUtc { get; set; }
    public string? LastUpdatedDatetimeMpt { get; set; }
    public int TotalMaxGeneration { get; set; }
    public int TotalNetGeneration { get; set; }
    public int NetToGridGeneration { get; set; }
    public int DispatchedContingencyReserve { get; set; }
    public int LssoPriceSettingDispatchedGeneration { get; set; }
    public int AlbertaInternalLoad { get; set; }
    public int ContingentEvents { get; set; }
    public List<AesoV2FuelTypeDetail>? FuelTypeDetails { get; set; }
}

public class AesoV2FuelTypeDetail
{
    public string FuelType { get; set; } = string.Empty;
    public int MaxGeneration { get; set; }
    public int NetGeneration { get; set; }
    public int DispatchedContingencyReserve { get; set; }
}

