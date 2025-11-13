// AlarmMessage.cs - Real-time alarm notification DTO
namespace Dashboard.Contracts.DTOs;

public record AlarmMessage
{
    public required Guid AlarmEventId { get; init; }
    public required string TagName { get; init; }
    public required string AlarmType { get; init; }
    public required string State { get; init; }
    public int Severity { get; init; }
    public double CurrentValue { get; init; }
    public double? Threshold { get; init; }
    public DateTime Timestamp { get; init; }
    public string? Message { get; init; }
}


