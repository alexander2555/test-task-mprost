namespace Api.Contracts.Reports;

// Строка проектного отчёта с данными по конкретному проекту
public sealed class ProjectReportRow
{
    public required string ProjectId { get; init; }

    public required string ProjectCode { get; init; }

    public required string ProjectName { get; init; }

    public decimal Hours { get; init; }

    public decimal Amount { get; init; }

    public decimal Budget { get; init; }

    public decimal? Percent { get; init; }

    // Превышен ли бюджет (>100%)
    public bool Overspent { get; init; }

    // Риск превышения бюджета (>80%)
    public bool Risk { get; init; }
}
