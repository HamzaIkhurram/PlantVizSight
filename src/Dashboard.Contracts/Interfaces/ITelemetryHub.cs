// ITelemetryHub.cs - SignalR hub interface for real-time telemetry distribution
using Dashboard.Contracts.DTOs;

namespace Dashboard.Contracts.Interfaces;

public interface ITelemetryHub
{
    Task BroadcastTelemetry(TelemetryMessage message);
    Task BroadcastAlarm(AlarmMessage message);
    Task SendToGroup(string groupName, TelemetryMessage message);
}


