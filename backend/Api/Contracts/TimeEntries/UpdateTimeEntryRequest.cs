namespace Api.Contracts.TimeEntries;

// Запрос на обновление записи табеля. Содержит все поля для обновления включая версию для оптимистической блокировки.
public sealed class UpdateTimeEntryRequest
{
    public string EmployeeId { get; init; } = string.Empty;

    public string ProjectId { get; init; } = string.Empty;

    public DateOnly? Date { get; init; }

    public decimal Hours { get; init; }

    public string? Comment { get; init; }

    // Версия записи для оптимистической блокировки (обязательно)
    public long Version { get; init; }
}
