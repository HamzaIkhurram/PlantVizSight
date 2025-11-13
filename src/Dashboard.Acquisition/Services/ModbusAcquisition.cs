// ModbusAcquisition.cs - Modbus TCP acquisition service that polls tags and broadcasts via SignalR
using Dashboard.Contracts.DTOs;
using Dashboard.Contracts.Interfaces;
using Dashboard.Domain.Models;
using Dashboard.Domain.Services;
using Microsoft.Extensions.Logging;

namespace Dashboard.Acquisition.Services;

public class ModbusAcquisition
{
    private readonly ITelemetryHub _telemetryHub;
    private readonly AlarmEngine _alarmEngine;
    private readonly ILogger<ModbusAcquisition> _logger;
    private readonly CancellationTokenSource _cts = new();
    private Task? _pollTask;
    private readonly Dictionary<int, float> _registerCache = new();

    public ModbusAcquisition(
        ITelemetryHub telemetryHub,
        AlarmEngine alarmEngine,
        ILogger<ModbusAcquisition> logger)
    {
        _telemetryHub = telemetryHub;
        _alarmEngine = alarmEngine;
        _logger = logger;
    }

    public void Start(List<Tag> tags, List<AlarmRule> alarmRules, Func<int, float> readRegister)
    {
        _pollTask = Task.Run(async () => await PollLoop(tags, alarmRules, readRegister, _cts.Token));
        _logger.LogInformation("Modbus acquisition started for {TagCount} tags", tags.Count);
    }

    public void Stop()
    {
        _cts.Cancel();
        _pollTask?.Wait(TimeSpan.FromSeconds(5));
        _logger.LogInformation("Modbus acquisition stopped");
    }

    private async Task PollLoop(
        List<Tag> tags, 
        List<AlarmRule> alarmRules, 
        Func<int, float> readRegister,
        CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                foreach (var tag in tags.Where(t => t.ReadRegister.HasValue))
                {
                    try
                    {
                        var rawValue = readRegister(tag.ReadRegister!.Value);
                        var scaledValue = tag.ApplyScaling(rawValue);
                        tag.CurrentValue = scaledValue;
                        tag.LastUpdate = DateTime.UtcNow;
                        tag.Quality = 192;

                        await _telemetryHub.BroadcastTelemetry(new TelemetryMessage
                        {
                            TagName = tag.Name,
                            Value = scaledValue,
                            Unit = tag.EngineeringUnit,
                            Timestamp = tag.LastUpdate,
                            Quality = tag.Quality,
                            Source = "Modbus"
                        });

                        var tagAlarms = alarmRules.Where(r => r.TagId == tag.TagId).ToList();
                        if (tagAlarms.Any())
                        {
                            await _alarmEngine.ProcessValue(tag, scaledValue, tagAlarms);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to poll tag: {TagName}", tag.Name);
                        tag.Quality = 0;
                    }
                }

                await Task.Delay(2000, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in poll loop");
                await Task.Delay(5000, ct);
            }
        }
    }
}


