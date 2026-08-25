using Api.Domain.Entities;
using MongoDB.Driver;

namespace Api.Infrastructure.Mongo;

// Контекст MongoDB - предоставляет доступ к коллекциям базы данных.
// Является единой точкой входа для работы с MongoDB в приложении.
public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;

    // Создаёт новый экземпляр контекста MongoDB
    public MongoDbContext(IMongoDatabase database)
    {
        _database = database;
    }

    // Коллекция сотрудников. Имя коллекции в MongoDB: "employees"
    public IMongoCollection<Employee> Employees =>
        _database.GetCollection<Employee>("employees");

    // Коллекция проектов. Имя коллекции в MongoDB: "projects"
    public IMongoCollection<Project> Projects =>
        _database.GetCollection<Project>("projects");

    // Коллекция записей о затраченном времени. Индексирована по дате, сотруднику и проекту.
    public IMongoCollection<TimeEntry> TimeEntries =>
        _database.GetCollection<TimeEntry>("time_entries");

    // Коллекция закрытых расчётных периодов. Индексирована по уникальной паре год-месяц.
    public IMongoCollection<ClosedPeriod> ClosedPeriods =>
        _database.GetCollection<ClosedPeriod>("closed_periods");
}
