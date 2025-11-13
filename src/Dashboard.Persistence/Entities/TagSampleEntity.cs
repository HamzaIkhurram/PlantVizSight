// TagSampleEntity.cs - Entity Framework entity for tag_sample table
namespace Dashboard.Persistence.Entities;

public class TagSampleEntity
{
    public DateTime Ts { get; set; }
    public Guid TagId { get; set; }
    public double Value { get; set; }
    public short Quality { get; set; } = 192;
    public TagEntity Tag { get; set; } = null!;
}



