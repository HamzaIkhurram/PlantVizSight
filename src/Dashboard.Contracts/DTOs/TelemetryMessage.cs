// TelemetryMessage.cs - Real-time telemetry message DTO for SignalR distribution
namespace Dashboard.Contracts.DTOs;

public record TelemetryMessage
{
    public required string TagName { get; init; }
    public double Value { get; init; }
    public string Unit { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public short Quality { get; init; } = 192;
    public string Source { get; init; } = "Modbus";
}


