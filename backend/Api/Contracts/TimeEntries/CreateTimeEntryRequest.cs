namespace Api.Contracts.TimeEntries;

// Запрос на создание записи табеля. Содержит данные о сотруднике, проекте, дате, часах и комментарии.
public sealed class CreateTimeEntryRequest
{
    public string EmployeeId { get; init; } = string.Empty;

    public string ProjectId { get; init; } = string.Empty;

    public DateOnly? Date { get; init; }

    public decimal Hours { get; init; }

    public string? Comment { get; init; }
}
