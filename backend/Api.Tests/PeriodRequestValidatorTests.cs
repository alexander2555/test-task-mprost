using Api.Application.Common;
using Api.Application.Periods;
using Api.Contracts.Periods;

namespace Api.Tests;

public sealed class PeriodRequestValidatorTests
{
    [Fact]
    public void Validate_accepts_valid_period()
    {
        PeriodRequestValidator.Validate(
            new PeriodRequest
            {
                Year = 2026,
                Month = 2
            });
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(10000, 2)]
    [InlineData(2026, 0)]
    [InlineData(2026, 13)]
    public void Validate_rejects_invalid_period(int year, int month)
    {
        var exception = Assert.Throws<ApiException>(
            () => PeriodRequestValidator.Validate(
                new PeriodRequest
                {
                    Year = year,
                    Month = month
                }));

        Assert.Equal("validation_error", exception.Code);
    }
}
