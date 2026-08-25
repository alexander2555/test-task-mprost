namespace Api.Contracts.TimeEntries;

public sealed class TimeEntryListItem
{
    public required string Id { get; init; }

    public required string EmployeeId { get; init; }

    public required string EmployeeName { get; init; }

    public required string ProjectId { get; init; }

    public required string ProjectCode { get; init; }

    public DateOnly Date { get; init; }

    public decimal Hours { get; init; }

    public decimal Rate { get; init; }

    public decimal Amount { get; init; }

    public bool Overtime { get; init; }

    public required string Comment { get; init; }

    public long Version { get; init; }
}
