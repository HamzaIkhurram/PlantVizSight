// AuditEventEntity.cs - Entity Framework entity for audit_event table
namespace Dashboard.Persistence.Entities;

public class AuditEventEntity
{
    public Guid AuditEventId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Ts { get; set; } = DateTime.UtcNow;
}



