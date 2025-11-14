// WellPairBroadcaster.cs - Service for broadcasting well pair updates through SignalR
using Dashboard.Contracts.DTOs;
using Dashboard.Domain.Models;
using Dashboard.Simulator;
using Dashboard.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Dashboard.Web.Services;

public class WellPairBroadcaster
{
    private readonly IHubContext<TelemetryHub> _hubContext;
    private readonly WellPairSimulator _simulator;
    private readonly ILogger<WellPairBroadcaster> _logger;
    private readonly CancellationTokenSource _cts = new();
    private Task? _broadcastTask;

    public WellPairBroadcaster(
        IHubContext<TelemetryHub> hubContext,
        WellPairSimulator simulator,
        ILogger<WellPairBroadcaster> logger)
    {
        _hubContext = hubContext;
        _simulator = simulator;
        _logger = logger;
    }

    public void Start()
    {
        _broadcastTask = Task.Run(async () => await BroadcastWellPairs(_cts.Token));
        _logger.LogInformation("Well pair broadcaster started");
    }

    public void Stop()
    {
        _cts.Cancel();
        _broadcastTask?.Wait(TimeSpan.FromSeconds(5));
        _logger.LogInformation("Well pair broadcaster stopped");
    }

    private async Task BroadcastWellPairs(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var wellPairs = _simulator.GetAllWellPairs();
                var wellPairMessages = wellPairs.Select(wp => new WellPairMessage
                {
                    WellPairName = wp.WellPairName,
                    Facility = wp.Facility,
                    Pad = wp.Pad,
                    SubcoolTemperature = wp.SubcoolTemperature,
                    WellheadPressure = wp.WellheadPressure,
                    ProductionRate = wp.ProductionRate,
                    SteamInjection = wp.SteamInjection,
                    SorRatio = wp.SorRatio,
                    SystemUptime = wp.SystemUptime,
                    Status = wp.Status,
                    Timestamp = wp.LastUpdate
                }).ToList();

                await _hubContext.Clients.All.SendAsync("ReceiveWellPairs", wellPairMessages, ct);

                var kpi = _simulator.GetKpiSummary();
                var kpiMessage = new WellPairKpiMessage
                {
                    TotalBitumenProduction = kpi.TotalBitumenProduction,
                    TotalSteamInjection = kpi.TotalSteamInjection,
                    AvgSubcoolTemperature = kpi.AvgSubcoolTemperature,
                    SystemUptime = kpi.SystemUptime,
                    AvgSor = kpi.AvgSor,
                    ActiveWellPairs = kpi.ActiveWellPairs,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("ReceiveWellPairKpi", kpiMessage, ct);

                await Task.Delay(1000, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting well pair updates");
                await Task.Delay(5000, ct);
            }
        }
    }
}

