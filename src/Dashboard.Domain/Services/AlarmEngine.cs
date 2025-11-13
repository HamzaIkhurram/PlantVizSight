// AlarmEngine.cs - Alarm processing engine with hysteresis and delay timers
using Dashboard.Contracts.DTOs;
using Dashboard.Contracts.Interfaces;
using Dashboard.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Dashboard.Domain.Services;

public class AlarmEngine
{
    private readonly Dictionary<Guid, AlarmState> _alarmStates = new();
    private readonly object _lock = new();
    private readonly ITelemetryHub? _telemetryHub;
    private readonly ILogger<AlarmEngine> _logger;

    public AlarmEngine(ITelemetryHub? telemetryHub, ILogger<AlarmEngine> logger)
    {
        _telemetryHub = telemetryHub;
        _logger = logger;
    }

    public async Task ProcessValue(Tag tag, double value, List<AlarmRule> rules)
    {
        var now = DateTime.UtcNow;

        foreach (var rule in rules.Where(r => r.Enabled))
        {
            lock (_lock)
            {
                if (!_alarmStates.ContainsKey(rule.AlarmRuleId))
                {
                    _alarmStates[rule.AlarmRuleId] = new AlarmState
                    {
                        AlarmRuleId = rule.AlarmRuleId
                    };
                }
            }

            var state = _alarmStates[rule.AlarmRuleId];
            var shouldTrip = rule.ShouldTrip(value, tag.SpanLow, tag.SpanHigh, state.IsActive);
            var shouldClear = rule.ShouldClear(value, tag.SpanLow, tag.SpanHigh);

            if (shouldTrip && !state.IsActive)
            {
                if (state.DelayOnStart == null)
                {
                    state.DelayOnStart = now;
                    _logger.LogDebug("Alarm delay-on started: {Tag} {Type}", tag.Name, rule.Type);
                }
                else if ((now - state.DelayOnStart.Value).TotalMilliseconds >= rule.DelayOnMs)
                {
                    state.IsActive = true;
                    state.TripTime = now;
                    state.DelayOnStart = null;
                    state.DelayOffStart = null;
                    state.IsAcknowledged = false;

                    _logger.LogWarning("Alarm TRIPPED: {Tag} {Type} Value={Value} Threshold={Threshold}",
                        tag.Name, rule.Type, value, rule.Threshold);

                    // Broadcast alarm
                    if (_telemetryHub != null)
                    {
                        await _telemetryHub.BroadcastAlarm(new AlarmMessage
                        {
                            AlarmEventId = Guid.NewGuid(),
                            TagName = tag.Name,
                            AlarmType = rule.Type,
                            State = "Active",
                            Severity = rule.Severity,
                            CurrentValue = value,
                            Threshold = rule.Threshold,
                            Timestamp = now,
                            Message = $"{tag.Name} {rule.Type} alarm: {value:F2} {tag.EngineeringUnit}"
                        });
                    }
                }
            }
            else if (!shouldTrip)
            {
                if (state.DelayOnStart != null)
                {
                    state.DelayOnStart = null;
                    _logger.LogDebug("Alarm delay-on cancelled: {Tag} {Type}", tag.Name, rule.Type);
                }
            }

            if (shouldClear && state.IsActive)
            {
                if (state.DelayOffStart == null)
                {
                    state.DelayOffStart = now;
                    _logger.LogDebug("Alarm delay-off started: {Tag} {Type}", tag.Name, rule.Type);
                }
                else if ((now - state.DelayOffStart.Value).TotalMilliseconds >= rule.DelayOffMs)
                {
                    state.IsActive = false;
                    state.ClearTime = now;
                    state.DelayOffStart = null;
                    state.DelayOnStart = null;

                    _logger.LogInformation("Alarm CLEARED: {Tag} {Type} Value={Value}",
                        tag.Name, rule.Type, value);

                    if (_telemetryHub != null)
                    {
                        await _telemetryHub.BroadcastAlarm(new AlarmMessage
                        {
                            AlarmEventId = Guid.NewGuid(),
                            TagName = tag.Name,
                            AlarmType = rule.Type,
                            State = state.IsAcknowledged ? "RTN" : "Cleared",
                            Severity = rule.Severity,
                            CurrentValue = value,
                            Threshold = rule.Threshold,
                            Timestamp = now,
                            Message = $"{tag.Name} {rule.Type} alarm cleared: {value:F2} {tag.EngineeringUnit}"
                        });
                    }
                }
            }
            else if (!shouldClear)
            {
                if (state.DelayOffStart != null)
                {
                    state.DelayOffStart = null;
                    _logger.LogDebug("Alarm delay-off cancelled: {Tag} {Type}", tag.Name, rule.Type);
                }
            }
        }
    }

    public async Task AcknowledgeAlarm(Guid alarmRuleId, string acknowledgedBy)
    {
        lock (_lock)
        {
            if (_alarmStates.TryGetValue(alarmRuleId, out var state) && state.IsActive)
            {
                state.IsAcknowledged = true;
                state.AckTime = DateTime.UtcNow;
                state.AckBy = acknowledgedBy;
                _logger.LogInformation("Alarm acknowledged by {User}: {AlarmId}", 
                    acknowledgedBy, alarmRuleId);
            }
        }

        await Task.CompletedTask;
    }

    public Dictionary<Guid, AlarmState> GetAlarmStates()
    {
        lock (_lock)
        {
            return new Dictionary<Guid, AlarmState>(_alarmStates);
        }
    }
}


