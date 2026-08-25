namespace Api.Contracts.TimeEntries;

// Ответ API с данными записи табеля. Возвращает все поля записи включая версию для оптимистической блокировки.
public sealed class TimeEntryResponse
{
    public required string Id { get; init; }

    public required string EmployeeId { get; init; }

    public required string ProjectId { get; init; }

    public DateOnly Date { get; init; }

    public decimal Hours { get; init; }

    public required string Comment { get; init; }

    // Версия записи для оптимистической блокировки
    public long Version { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime UpdatedAtUtc { get; init; }
}
