using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Api.Domain.Entities;

// Запись о ставке сотрудника. Используется для определения актуальной ставки на конкретную дату.
// Ставка актуальна для записи о времени, если Date записи >= From и < следующей записи From.
public sealed class EmployeeRate
{
    // Дата начала действия ставки. Все записи о времени с этой даты используют эту ставку.
    [BsonDateOnlyOptions(BsonType.DateTime)]
    public DateOnly From { get; set; }

    // Почасовая ставка в денежном выражении
    public decimal Value { get; set; }
}
