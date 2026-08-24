using Api.Domain.Entities;
using MongoDB.Driver;

namespace Api.Infrastructure.Mongo;

public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoDatabase database)
    {
        _database = database;
    }

    public IMongoCollection<Employee> Employees =>
        _database.GetCollection<Employee>("employees");

    public IMongoCollection<Project> Projects =>
        _database.GetCollection<Project>("projects");

    public IMongoCollection<TimeEntry> TimeEntries =>
        _database.GetCollection<TimeEntry>("time_entries");

    public IMongoCollection<ClosedPeriod> ClosedPeriods =>
        _database.GetCollection<ClosedPeriod>("closed_periods");
}
