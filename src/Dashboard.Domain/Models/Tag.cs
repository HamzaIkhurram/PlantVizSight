// Tag.cs - Represents a process variable (sensor, actuator, etc.)
namespace Dashboard.Domain.Models;

public class Tag
{
    public Guid TagId { get; set; }
    public required string Name { get; set; }
    public required string EngineeringUnit { get; set; }
    public double Scale { get; set; } = 1.0;
    public double Offset { get; set; } = 0.0;
    public double SpanLow { get; set; } = 0.0;
    public double SpanHigh { get; set; } = 100.0;
    public required string DataType { get; set; }
    public int? ReadRegister { get; set; }
    public int? WriteRegister { get; set; }
    public string Site { get; set; } = "default";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public double CurrentValue { get; set; }
    public short Quality { get; set; } = 192;
    public DateTime LastUpdate { get; set; }

    public double ApplyScaling(double rawValue)
    {
        return (rawValue * Scale) + Offset;
    }

    public bool IsWithinSpan(double value)
    {
        return value >= SpanLow && value <= SpanHigh;
    }
}


