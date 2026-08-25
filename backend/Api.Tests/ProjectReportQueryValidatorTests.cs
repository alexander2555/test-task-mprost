using Api.Application.Common;
using Api.Application.Reports;
using Api.Contracts.Reports;

namespace Api.Tests;

public sealed class ProjectReportQueryValidatorTests
{
    [Fact]
    public void Validate_accepts_valid_period()
    {
        ProjectReportQueryValidator.Validate(new ProjectReportQuery { Year = 2026, Month = 3 });
    }

    [Theory]
    [InlineData(0, 3)]
    [InlineData(9999, 3)]
    [InlineData(2026, 0)]
    [InlineData(2026, 13)]
    public void Validate_rejects_invalid_period(int year, int month)
    {
        var exception = Assert.Throws<ApiException>(() =>
            ProjectReportQueryValidator.Validate(new ProjectReportQuery { Year = year, Month = month }));

        Assert.Equal("validation_error", exception.Code);
    }
}
