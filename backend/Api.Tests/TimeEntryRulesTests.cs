using Api.Application.Common;
using Api.Application.TimeEntries;
using Api.Domain.Entities;

namespace Api.Tests;

public sealed class TimeEntryRulesTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-0.5)]
    [InlineData(24.5)]
    [InlineData(0.25)]
    [InlineData(7.75)]
    public void EnsureValidHours_rejects_invalid_values(decimal hours)
    {
        var exception = Assert.Throws<ApiException>(
            () => TimeEntryRules.EnsureValidHours(hours));

        Assert.Equal("validation_error", exception.Code);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(8)]
    [InlineData(12.5)]
    [InlineData(24)]
    public void EnsureValidHours_accepts_valid_values(decimal hours)
    {
        TimeEntryRules.EnsureValidHours(hours);
    }

    [Fact]
    public void EnsureDateWithinProject_accepts_start_and_end_boundaries()
    {
        var project = Project(
            new DateOnly(2026, 2, 1),
            new DateOnly(2026, 2, 28));

        TimeEntryRules.EnsureDateWithinProject(
            project,
            new DateOnly(2026, 2, 1));

        TimeEntryRules.EnsureDateWithinProject(
            project,
            new DateOnly(2026, 2, 28));
    }

    [Theory]
    [InlineData(2026, 1, 31)]
    [InlineData(2026, 3, 1)]
    public void EnsureDateWithinProject_rejects_date_outside_project(
        int year,
        int month,
        int day)
    {
        var project = Project(
            new DateOnly(2026, 2, 1),
            new DateOnly(2026, 2, 28));

        var exception = Assert.Throws<ApiException>(
            () => TimeEntryRules.EnsureDateWithinProject(
                project,
                new DateOnly(year, month, day)));

        Assert.Equal("time_entry_outside_project_period", exception.Code);
    }

    [Fact]
    public void EnsureDateWithinProject_accepts_date_after_start_for_open_ended_project()
    {
        var project = Project(new DateOnly(2026, 2, 1), null);

        TimeEntryRules.EnsureDateWithinProject(
            project,
            new DateOnly(2030, 1, 1));
    }

    [Fact]
    public void EnsureDailyHoursWithinLimit_accepts_exactly_24_hours()
    {
        TimeEntryRules.EnsureDailyHoursWithinLimit(16m, 8m);
    }

    [Fact]
    public void EnsureDailyHoursWithinLimit_rejects_more_than_24_hours()
    {
        var exception = Assert.Throws<ApiException>(
            () => TimeEntryRules.EnsureDailyHoursWithinLimit(16.5m, 8m));

        Assert.Equal("daily_hours_limit_exceeded", exception.Code);
    }

    [Fact]
    public void EnsurePeriodOpen_rejects_closed_period()
    {
        var exception = Assert.Throws<ApiException>(
            () => TimeEntryRules.EnsurePeriodOpen(true));

        Assert.Equal("period_closed", exception.Code);
    }

    [Fact]
    public void EnsureVersionMatches_rejects_stale_version()
    {
        var exception = Assert.Throws<ApiException>(
            () => TimeEntryRules.EnsureVersionMatches(3, 2));

        Assert.Equal("time_entry_version_conflict", exception.Code);
    }

    private static Project Project(DateOnly startDate, DateOnly? endDate)
    {
        return new Project
        {
            StartDate = startDate,
            EndDate = endDate
        };
    }
}
