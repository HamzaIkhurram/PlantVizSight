// DashboardDbContext.cs - Entity Framework DbContext for PlantSight database
using Dashboard.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Persistence;

public class DashboardDbContext : DbContext
{
    public DashboardDbContext(DbContextOptions<DashboardDbContext> options)
        : base(options)
    {
    }

    public DbSet<TagEntity> Tags => Set<TagEntity>();
    public DbSet<TagSampleEntity> TagSamples => Set<TagSampleEntity>();
    public DbSet<AlarmRuleEntity> AlarmRules => Set<AlarmRuleEntity>();
    public DbSet<AlarmEventEntity> AlarmEvents => Set<AlarmEventEntity>();
    public DbSet<AuditEventEntity> AuditEvents => Set<AuditEventEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TagEntity>(entity =>
        {
            entity.ToTable("tag");
            entity.HasKey(e => e.TagId);
            entity.Property(e => e.TagId).HasColumnName("tag_id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Eu).HasColumnName("eu").IsRequired();
            entity.Property(e => e.Scale).HasColumnName("scale");
            entity.Property(e => e.Offset).HasColumnName("offset");
            entity.Property(e => e.SpanLow).HasColumnName("span_low");
            entity.Property(e => e.SpanHigh).HasColumnName("span_high");
            entity.Property(e => e.DataType).HasColumnName("datatype").IsRequired();
            entity.Property(e => e.ReadRegister).HasColumnName("read_register");
            entity.Property(e => e.WriteRegister).HasColumnName("write_register");
            entity.Property(e => e.Site).HasColumnName("site");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<TagSampleEntity>(entity =>
        {
            entity.ToTable("tag_sample");
            entity.HasKey(e => new { e.Ts, e.TagId });
            entity.Property(e => e.Ts).HasColumnName("ts");
            entity.Property(e => e.TagId).HasColumnName("tag_id");
            entity.Property(e => e.Value).HasColumnName("value");
            entity.Property(e => e.Quality).HasColumnName("quality");

            entity.HasOne(e => e.Tag)
                .WithMany(t => t.Samples)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TagId, e.Ts });
        });

        modelBuilder.Entity<AlarmRuleEntity>(entity =>
        {
            entity.ToTable("alarm_rule");
            entity.HasKey(e => e.AlarmRuleId);
            entity.Property(e => e.AlarmRuleId).HasColumnName("alarm_rule_id");
            entity.Property(e => e.TagId).HasColumnName("tag_id");
            entity.Property(e => e.Type).HasColumnName("type").IsRequired();
            entity.Property(e => e.Threshold).HasColumnName("threshold");
            entity.Property(e => e.Severity).HasColumnName("severity");
            entity.Property(e => e.HystPct).HasColumnName("hyst_pct");
            entity.Property(e => e.DelayOnMs).HasColumnName("delay_on_ms");
            entity.Property(e => e.DelayOffMs).HasColumnName("delay_off_ms");

            entity.HasOne(e => e.Tag)
                .WithMany(t => t.AlarmRules)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.TagId);
        });

        modelBuilder.Entity<AlarmEventEntity>(entity =>
        {
            entity.ToTable("alarm_event");
            entity.HasKey(e => e.AlarmEventId);
            entity.Property(e => e.AlarmEventId).HasColumnName("alarm_event_id");
            entity.Property(e => e.AlarmRuleId).HasColumnName("alarm_rule_id");
            entity.Property(e => e.TagId).HasColumnName("tag_id");
            entity.Property(e => e.TsStart).HasColumnName("ts_start");
            entity.Property(e => e.TsEnd).HasColumnName("ts_end");
            entity.Property(e => e.State).HasColumnName("state").IsRequired();
            entity.Property(e => e.AckBy).HasColumnName("ack_by");
            entity.Property(e => e.AckTs).HasColumnName("ack_ts");
            entity.Property(e => e.Message).HasColumnName("message");

            entity.HasOne(e => e.AlarmRule)
                .WithMany(r => r.Events)
                .HasForeignKey(e => e.AlarmRuleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Tag)
                .WithMany()
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.TagId, e.TsStart });
        });

        modelBuilder.Entity<AuditEventEntity>(entity =>
        {
            entity.ToTable("audit_event");
            entity.HasKey(e => e.AuditEventId);
            entity.Property(e => e.AuditEventId).HasColumnName("audit_event_id");
            entity.Property(e => e.UserName).HasColumnName("user_name");
            entity.Property(e => e.Action).HasColumnName("action").IsRequired();
            entity.Property(e => e.Details).HasColumnName("details").HasColumnType("jsonb");
            entity.Property(e => e.Ts).HasColumnName("ts");

            entity.HasIndex(e => e.Ts);
        });
    }
}



