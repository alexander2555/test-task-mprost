namespace Api.Contracts.Reports;

// Запрос на генерацию проектного отчёта за указанный год и месяц
public sealed class ProjectReportQuery
{
    public int Year { get; init; }

    public int Month { get; init; }
}
