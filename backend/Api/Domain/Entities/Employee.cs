using MongoDB.Bson;

namespace Api.Domain.Entities;

// Сущность сотрудника. Хранит информацию о работнике и историю его почасовых ставок.
// Rates хранится как embedded-массив, так как история ставок небольшая и редко меняется.
public sealed class Employee
{
    // Уникальный идентификатор сотрудника в MongoDB (ObjectId)
    public ObjectId Id { get; set; }

    // Полное имя сотрудника
    public string FullName { get; set; } = string.Empty;

    // Отдел/подразделение сотрудника
    public string Department { get; set; } = string.Empty;

    // История почасовых ставок сотрудника. Ставка определяется как актуальная на дату записи.
    public List<EmployeeRate> Rates { get; set; } = [];
}
