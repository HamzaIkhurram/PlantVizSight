// WellPairSimulator.cs - Simulator for 66 well pairs across 2 facilities and 16 pads
using Dashboard.Domain.Models;

namespace Dashboard.Simulator;

public class WellPairSimulator
{
    private readonly List<WellPair> _wellPairs = new();
    private readonly Dictionary<string, double> _trends = new();
    private readonly Dictionary<string, double> _timeSteps = new();
    private readonly Random _random = new();
    private readonly CancellationTokenSource _cts = new();
    private Task? _simulationTask;

    public WellPairSimulator()
    {
        InitializeWellPairs();
    }

    private void InitializeWellPairs()
    {
        var facilities = new[] { "Facility A", "Facility B" };
        int totalPairs = 0;

        for (int facilityIdx = 0; facilityIdx < facilities.Length; facilityIdx++)
        {
            var facility = facilities[facilityIdx];
            var pairCount = facilityIdx == 0 ? 31 : 35;
            var padsPerFacility = 8;
            var basePairsPerPad = pairCount / padsPerFacility;
            var extraPairs = pairCount % padsPerFacility;

            for (int padNum = 1; padNum <= padsPerFacility; padNum++)
            {
                var padName = $"Pad {padNum:D2}";
                var pairsPerPad = padNum <= extraPairs ? basePairsPerPad + 1 : basePairsPerPad;

                for (int pair = 1; pair <= pairsPerPad; pair++)
                {
                    var wellPair = new WellPair
                    {
                        WellPairId = Guid.NewGuid(),
                        WellPairName = $"WP-{facility.Substring(facility.Length - 1)}-{padNum:D2}-{pair:D2}",
                        Facility = facility,
                        Pad = padName,
                        SubcoolTemperature = 185 + (_random.NextDouble() - 0.5) * 10,
                        WellheadPressure = 9000 + (_random.NextDouble() - 0.5) * 500,
                        ProductionRate = facilityIdx == 0 
                            ? 383.4 + (_random.NextDouble() - 0.5) * 100
                            : 360.8 + (_random.NextDouble() - 0.5) * 100,
                        SteamInjection = 1180 + (_random.NextDouble() - 0.5) * 200,
                        SorRatio = facilityIdx == 0 ? 3.20 : 3.19,
                        SystemUptime = 94.0 + _random.NextDouble() * 2,
                        LastUpdate = DateTime.UtcNow,
                        Status = "Normal"
                    };

                    _wellPairs.Add(wellPair);
                    var key = wellPair.WellPairName;
                    _trends[key] = (_random.NextDouble() - 0.5) * 20;
                    _timeSteps[key] = _random.NextDouble() * Math.PI * 2;

                    totalPairs++;
                }
            }
        }
    }

    public void Start()
    {
        _simulationTask = Task.Run(async () => await SimulateWellPairs(_cts.Token));
    }

    public void Stop()
    {
        _cts.Cancel();
        _simulationTask?.Wait(TimeSpan.FromSeconds(5));
    }

    private async Task SimulateWellPairs(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                foreach (var wellPair in _wellPairs)
                {
                    var key = wellPair.WellPairName;
                    if (!_timeSteps.ContainsKey(key))
                    {
                        _timeSteps[key] = 0;
                        _trends[key] = 0;
                    }

                    _timeSteps[key] += 0.01;
                    _trends[key] += (_random.NextDouble() - 0.5) * 2;
                    _trends[key] = Math.Clamp(_trends[key], -30, 30);

                    var facilityBase = wellPair.Facility == "Facility A" ? 383.4 : 360.8;
                    var productionBase = facilityBase;
                    var productionSine = Math.Sin(_timeSteps[key] * 0.5) * 30;
                    var productionNoise = (_random.NextDouble() - 0.5) * 20;
                    wellPair.ProductionRate = Math.Clamp(productionBase + productionSine + _trends[key] + productionNoise, 250, 500);

                    var subcoolBase = 187.0;
                    var subcoolSine = Math.Sin(_timeSteps[key] * 0.3) * 8;
                    var subcoolNoise = (_random.NextDouble() - 0.5) * 3;
                    wellPair.SubcoolTemperature = Math.Clamp(subcoolBase + subcoolSine + subcoolNoise, 175, 205);

                    var pressureBase = 9000.0;
                    var pressureSine = Math.Sin(_timeSteps[key] * 0.4) * 150;
                    var pressureNoise = (_random.NextDouble() - 0.5) * 100;
                    var pressureControl = wellPair.Facility == "Facility A" ? 150 : 200;
                    wellPair.WellheadPressure = Math.Clamp(pressureBase + pressureSine + pressureNoise, 8000, 10500);

                    var steamBase = 1180.0;
                    var steamSine = Math.Sin(_timeSteps[key] * 0.6) * 80;
                    var steamNoise = (_random.NextDouble() - 0.5) * 50;
                    wellPair.SteamInjection = Math.Clamp(steamBase + steamSine + steamNoise, 1000, 1400);

                    if (wellPair.ProductionRate > 0)
                    {
                        wellPair.SorRatio = wellPair.SteamInjection / wellPair.ProductionRate;
                    }
                    else
                    {
                        wellPair.SorRatio = wellPair.Facility == "Facility A" ? 3.20 : 3.19;
                    }

                    var uptimeBase = wellPair.Facility == "Facility A" ? 94.6 : 94.2;
                    var uptimeNoise = (_random.NextDouble() - 0.5) * 1;
                    wellPair.SystemUptime = Math.Clamp(uptimeBase + uptimeNoise, 90, 100);

                    if (wellPair.SubcoolTemperature > 210 || wellPair.SubcoolTemperature < 170 ||
                        wellPair.WellheadPressure > 10500 || wellPair.ProductionRate < wellPair.ProductionRate * 0.8 ||
                        wellPair.SystemUptime < 90)
                    {
                        wellPair.Status = "Alert";
                    }
                    else if (wellPair.SubcoolTemperature > 205 || wellPair.SubcoolTemperature < 175 ||
                             wellPair.WellheadPressure > 10200 || wellPair.SystemUptime < 92)
                    {
                        wellPair.Status = "Caution";
                    }
                    else
                    {
                        wellPair.Status = "Normal";
                    }

                    wellPair.LastUpdate = DateTime.UtcNow;
                }

                await Task.Delay(1000, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    public List<WellPair> GetAllWellPairs()
    {
        return new List<WellPair>(_wellPairs);
    }

    public WellPair? GetWellPair(string wellPairName)
    {
        return _wellPairs.FirstOrDefault(wp => wp.WellPairName == wellPairName);
    }

    public WellPairKpi GetKpiSummary()
    {
        var allPairs = GetAllWellPairs();
        return new WellPairKpi
        {
            TotalBitumenProduction = allPairs.Sum(wp => wp.ProductionRate),
            TotalSteamInjection = allPairs.Sum(wp => wp.SteamInjection),
            AvgSubcoolTemperature = allPairs.Average(wp => wp.SubcoolTemperature),
            SystemUptime = allPairs.Average(wp => wp.SystemUptime),
            AvgSor = allPairs.Average(wp => wp.SorRatio),
            ActiveWellPairs = allPairs.Count
        };
    }

    public List<FacilitySummary> GetFacilitySummaries()
    {
        var facilityA = _wellPairs.Where(wp => wp.Facility == "Facility A").ToList();
        var facilityB = _wellPairs.Where(wp => wp.Facility == "Facility B").ToList();

        return new List<FacilitySummary>
        {
            new FacilitySummary
            {
                FacilityName = "Facility A",
                AvgProduction = facilityA.Average(wp => wp.ProductionRate),
                AvgSor = facilityA.Average(wp => wp.SorRatio),
                PressureControl = 150,
                Uptime = facilityA.Average(wp => wp.SystemUptime),
                WellPairCount = facilityA.Count
            },
            new FacilitySummary
            {
                FacilityName = "Facility B",
                AvgProduction = facilityB.Average(wp => wp.ProductionRate),
                AvgSor = facilityB.Average(wp => wp.SorRatio),
                PressureControl = 200,
                Uptime = facilityB.Average(wp => wp.SystemUptime),
                WellPairCount = facilityB.Count
            }
        };
    }
}

