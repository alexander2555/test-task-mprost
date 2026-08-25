using MongoDB.Bson;

namespace Api.Domain.Entities;

// Закрытый расчётный период. После закрытия записи о времени за этот период нельзя изменять.
// На один календарный месяц может быть только один закрытый период (ограничение enforced через уникальный индекс).
public sealed class ClosedPeriod
{
    // Уникальный идентификатор закрытого периода в MongoDB (ObjectId)
    public ObjectId Id { get; set; }

    // Год закрытого периода (например, 2026)
    public int Year { get; set; }

    // Месяц закрытого периода (1-12)
    public int Month { get; set; }

    // Время закрытия периода в UTC. Фиксирует момент, когда период был закрыт.
    public DateTime ClosedAtUtc { get; set; }
}
