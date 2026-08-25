namespace Api.Contracts.ReferenceData;

public sealed class ProjectReferenceItem
{
    public required string Id { get; init; }

    public required string Code { get; init; }

    public required string Name { get; init; }
}
