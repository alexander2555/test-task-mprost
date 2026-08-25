namespace Api.Contracts.TimeEntries;

public sealed class TimeEntryQueryRequest
{
    public int Year { get; init; }

    public int Month { get; init; }

    public string? EmployeeId { get; init; }

    public string? ProjectId { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
