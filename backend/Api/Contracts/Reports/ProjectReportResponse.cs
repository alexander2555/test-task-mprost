namespace Api.Contracts.Reports;

// Ответ API с проектным отчётом. Содержит строки по проектам и итоговые суммы.
public sealed class ProjectReportResponse
{
    public required IReadOnlyList<ProjectReportRow> Rows { get; init; }

    public required ProjectReportTotals Totals { get; init; }
}
