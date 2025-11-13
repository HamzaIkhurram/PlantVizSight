// AlarmState.cs - Runtime alarm state with delay timers
namespace Dashboard.Domain.Models;

public class AlarmState
{
    public Guid AlarmRuleId { get; set; }
    public bool IsActive { get; set; }
    public bool IsAcknowledged { get; set; }
    public DateTime? TripTime { get; set; }
    public DateTime? ClearTime { get; set; }
    public DateTime? AckTime { get; set; }
    public string? AckBy { get; set; }
    public DateTime? DelayOnStart { get; set; }
    public DateTime? DelayOffStart { get; set; }

    public string GetState()
    {
        if (IsActive && !IsAcknowledged) return "Active";
        if (IsActive && IsAcknowledged) return "Acked";
        if (!IsActive && !IsAcknowledged) return "RTN";
        return "Normal";
    }
}


