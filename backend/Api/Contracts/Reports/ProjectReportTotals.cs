namespace Api.Contracts.Reports;

// Итоговые суммы проектного отчёта по всем проектам
public sealed class ProjectReportTotals
{
    public decimal Hours { get; init; }

    public decimal Amount { get; init; }

    public decimal Budget { get; init; }

    public decimal? Percent { get; init; }

    // Превышен ли общий бюджет (>100%)
    public bool Overspent { get; init; }

    // Риск превышения общего бюджета (>80%)
    public bool Risk { get; init; }
}
