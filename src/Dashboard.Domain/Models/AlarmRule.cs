// AlarmRule.cs - Defines alarm threshold and behavior
namespace Dashboard.Domain.Models;

public class AlarmRule
{
    public Guid AlarmRuleId { get; set; }
    public Guid TagId { get; set; }
    public required string Type { get; set; }
    public double? Threshold { get; set; }
    public short Severity { get; set; }
    public double HysteresisPct { get; set; } = 0.5;
    public int DelayOnMs { get; set; } = 2000;
    public int DelayOffMs { get; set; } = 5000;
    public bool Enabled { get; set; } = true;
    public Tag? Tag { get; set; }

    public double CalculateHysteresis(double spanLow, double spanHigh)
    {
        var span = spanHigh - spanLow;
        return span * (HysteresisPct / 100.0);
    }

    public bool ShouldTrip(double value, double spanLow, double spanHigh, bool currentlyActive)
    {
        if (!Enabled || Threshold == null) return false;

        var hysteresis = CalculateHysteresis(spanLow, spanHigh);

        return Type switch
        {
            "HH" => value > Threshold.Value,
            "H" => value > Threshold.Value,
            "L" => value < Threshold.Value,
            "LL" => value < Threshold.Value,
            _ => false
        };
    }

    public bool ShouldClear(double value, double spanLow, double spanHigh)
    {
        if (!Enabled || Threshold == null) return true;

        var hysteresis = CalculateHysteresis(spanLow, spanHigh);

        return Type switch
        {
            "HH" => value < (Threshold.Value - hysteresis),
            "H" => value < (Threshold.Value - hysteresis),
            "L" => value > (Threshold.Value + hysteresis),
            "LL" => value > (Threshold.Value + hysteresis),
            _ => true
        };
    }
}


