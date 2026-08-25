using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Api.Domain.Entities;

// Сущность проекта. Хранит информацию о проекте с уникальным шифром, бюджетом и периодом выполнения.
// Шифр проекта (Code) должен быть уникальным (ограничение enforced через уникальный индекс).
public sealed class Project
{
    // Уникальный идентификатор проекта в MongoDB (ObjectId)
    public ObjectId Id { get; set; }

    // Уникальный шифр проекта (например, "PRJ-001"). Индексирован как unique.
    public string Code { get; set; } = string.Empty;

    // Название проекта
    public string Name { get; set; } = string.Empty;

    // Бюджет проекта в денежном выражении
    public decimal Budget { get; set; }

    // Дата начала проекта. Хранится как DateTime в MongoDB для поддержки range queries и индексов.
    [BsonDateOnlyOptions(BsonType.DateTime)]
    public DateOnly StartDate { get; set; }

    // Дата окончания проекта (опционально). Null если проект всё ещё активен.
    [BsonDateOnlyOptions(BsonType.DateTime)]
    public DateOnly? EndDate { get; set; }
}
