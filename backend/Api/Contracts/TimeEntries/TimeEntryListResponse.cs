namespace Api.Contracts.TimeEntries;

public sealed class TimeEntryListResponse
{
    public required IReadOnlyList<TimeEntryListItem> Items { get; init; }

    public int Page { get; init; }

    public int PageSize { get; init; }

    public long TotalCount { get; init; }

    public decimal TotalHours { get; init; }

    public decimal TotalAmount { get; init; }
}
