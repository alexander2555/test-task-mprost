using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Api.Domain.Entities;

// Запись о затраченном времени. Ставка и стоимость вычисляются динамически на основе актуальной истории ставок.
// Version используется для оптимистической блокировки при обновлении записей.
public sealed class TimeEntry
{
    // Уникальный идентификатор записи в MongoDB (ObjectId)
    public ObjectId Id { get; set; }

    // Ссылка на сотрудника, который выполнил работу
    public ObjectId EmployeeId { get; set; }

    // Ссылка на проект, над которым выполнялась работа
    public ObjectId ProjectId { get; set; }

    // Дата выполнения работы. Используется для определения актуальной ставки сотрудника.
    [BsonDateOnlyOptions(BsonType.DateTime)]
    public DateOnly Date { get; set; }

    // Количество затраченных часов
    public decimal Hours { get; set; }

    // Комментарий к выполненной работе
    public string Comment { get; set; } = string.Empty;

    // Версия записи для оптимистической блокировки. Увеличивается при каждом обновлении.
    public long Version { get; set; }

    // Время создания записи в UTC
    public DateTime CreatedAtUtc { get; set; }

    // Время последнего обновления записи в UTC
    public DateTime UpdatedAtUtc { get; set; }
}
