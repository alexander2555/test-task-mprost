namespace Api.Contracts.ReferenceData;

public sealed class EmployeeReferenceItem
{
    public required string Id { get; init; }

    public required string FullName { get; init; }

    public required string Department { get; init; }
}
