// AlarmEventEntity.cs - Entity Framework entity for alarm_event table
namespace Dashboard.Persistence.Entities;

public class AlarmEventEntity
{
    public Guid AlarmEventId { get; set; }
    public Guid AlarmRuleId { get; set; }
    public Guid TagId { get; set; }
    public DateTime TsStart { get; set; }
    public DateTime? TsEnd { get; set; }
    public string State { get; set; } = string.Empty;
    public string? AckBy { get; set; }
    public DateTime? AckTs { get; set; }
    public string? Message { get; set; }
    public AlarmRuleEntity AlarmRule { get; set; } = null!;
    public TagEntity Tag { get; set; } = null!;
}



