// TelemetryHub.cs - SignalR hub for real-time telemetry and alarm distribution
using Dashboard.Contracts.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace Dashboard.Web.Hubs;

public class TelemetryHub : Hub
{
    private readonly ILogger<TelemetryHub> _logger;

    public TelemetryHub(ILogger<TelemetryHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeToSite(string siteName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, siteName);
        _logger.LogInformation("Client {ConnectionId} subscribed to site: {Site}", 
            Context.ConnectionId, siteName);
    }

    public async Task UnsubscribeFromSite(string siteName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, siteName);
        _logger.LogInformation("Client {ConnectionId} unsubscribed from site: {Site}", 
            Context.ConnectionId, siteName);
    }

    public async Task RequestSnapshot()
    {
        _logger.LogInformation("Snapshot requested by client: {ConnectionId}", Context.ConnectionId);
        await Task.CompletedTask;
    }
}


