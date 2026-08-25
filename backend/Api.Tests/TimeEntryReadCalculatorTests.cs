using Api.Application.TimeEntries;

namespace Api.Tests;

public sealed class TimeEntryReadCalculatorTests
{
    [Fact]
    public void CalculateAmount_uses_midpoint_rounding_to_even()
    {
        var amount = TimeEntryReadCalculator.CalculateAmount(0.5m, 1.01m);

        Assert.Equal(0.50m, amount);
    }

    [Fact]
    public void CalculateAmount_rounds_non_midpoint_value_to_two_decimals()
    {
        var amount = TimeEntryReadCalculator.CalculateAmount(1.25m, 10.11m);

        Assert.Equal(12.64m, amount);
    }
}
