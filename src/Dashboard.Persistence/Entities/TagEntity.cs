// TagEntity.cs - Entity Framework entity for tag table
namespace Dashboard.Persistence.Entities;

public class TagEntity
{
    public Guid TagId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Eu { get; set; } = string.Empty;
    public double Scale { get; set; } = 1.0;
    public double Offset { get; set; } = 0.0;
    public double SpanLow { get; set; } = 0.0;
    public double SpanHigh { get; set; } = 100.0;
    public string DataType { get; set; } = "float";
    public int? ReadRegister { get; set; }
    public int? WriteRegister { get; set; }
    public string Site { get; set; } = "default";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<TagSampleEntity> Samples { get; set; } = new List<TagSampleEntity>();
    public ICollection<AlarmRuleEntity> AlarmRules { get; set; } = new List<AlarmRuleEntity>();
}



