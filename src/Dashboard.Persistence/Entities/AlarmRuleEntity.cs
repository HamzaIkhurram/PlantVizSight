// AlarmRuleEntity.cs - Entity Framework entity for alarm_rule table
namespace Dashboard.Persistence.Entities;

public class AlarmRuleEntity
{
    public Guid AlarmRuleId { get; set; }
    public Guid? TagId { get; set; }
    public string Type { get; set; } = string.Empty;
    public double? Threshold { get; set; }
    public short Severity { get; set; }
    public double HystPct { get; set; } = 0.5;
    public int DelayOnMs { get; set; } = 2000;
    public int DelayOffMs { get; set; } = 5000;
    public TagEntity? Tag { get; set; }
    public ICollection<AlarmEventEntity> Events { get; set; } = new List<AlarmEventEntity>();
}



