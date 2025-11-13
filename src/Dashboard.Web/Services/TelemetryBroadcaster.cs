// TelemetryBroadcaster.cs - Service for broadcasting telemetry through SignalR
using Dashboard.Contracts.DTOs;
using Dashboard.Contracts.Interfaces;
using Dashboard.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Dashboard.Web.Services;

public class TelemetryBroadcaster : ITelemetryHub
{
    private readonly IHubContext<TelemetryHub> _hubContext;
    private readonly ILogger<TelemetryBroadcaster> _logger;

    public TelemetryBroadcaster(
        IHubContext<TelemetryHub> hubContext,
        ILogger<TelemetryBroadcaster> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task BroadcastTelemetry(TelemetryMessage message)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveTelemetry", message);
            _logger.LogTrace("Broadcasted telemetry for tag: {TagName}", message.TagName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast telemetry for tag: {TagName}", message.TagName);
        }
    }

    public async Task BroadcastAlarm(AlarmMessage message)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveAlarm", message);
            _logger.LogInformation("Broadcasted alarm: {TagName} - {State}", 
                message.TagName, message.State);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast alarm for tag: {TagName}", message.TagName);
        }
    }

    public async Task SendToGroup(string groupName, TelemetryMessage message)
    {
        try
        {
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveTelemetry", message);
            _logger.LogTrace("Sent telemetry to group {Group}: {TagName}", 
                groupName, message.TagName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send telemetry to group {Group}", groupName);
        }
    }
}


