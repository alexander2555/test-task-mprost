using Api.Application.TimeEntries;
using Api.Domain.Entities;

namespace Api.Tests;

public sealed class RateResolverTests
{
    private static readonly IReadOnlyCollection<EmployeeRate> Rates =
    [
        new() { From = new DateOnly(2026, 1, 1), Value = 500m },
        new() { From = new DateOnly(2026, 3, 1), Value = 600m }
    ];

    [Theory]
    [InlineData(2026, 1, 1, 500)]
    [InlineData(2026, 2, 28, 500)]
    [InlineData(2026, 3, 1, 600)]
    [InlineData(2026, 3, 15, 600)]
    public void Resolve_returns_latest_rate_not_after_entry_date(
        int year,
        int month,
        int day,
        decimal expected)
    {
        var result = RateResolver.Resolve(Rates, new DateOnly(year, month, day));

        Assert.NotNull(result);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void Resolve_returns_null_when_no_rate_is_effective_yet()
    {
        var result = RateResolver.Resolve(
            Rates,
            new DateOnly(2025, 12, 31));

        Assert.Null(result);
    }
}
