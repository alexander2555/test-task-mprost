using Api.Domain.Entities;
using MongoDB.Driver;

namespace Api.Infrastructure.Mongo;

// Инициализатор индексов MongoDB. Создаёт все необходимые индексы при запуске приложения.
// Индексы критически важны для производительности запросов к большим коллекциям.
public sealed class MongoIndexInitializer
{
    private readonly MongoDbContext _db;

    // Создаёт новый экземпляр инициализатора индексов
    public MongoIndexInitializer(MongoDbContext db)
    {
        _db = db;
    }

    // Создаёт все индексы для всех коллекций. Если индексы уже существуют, MongoDB пропустит их создание.
    public async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
    {
        await CreateTimeEntryIndexesAsync(cancellationToken);
        await CreateProjectIndexesAsync(cancellationToken);
        await CreateClosedPeriodIndexesAsync(cancellationToken);
    }

    // Создаёт индексы для коллекции записей о времени: по дате, по сотруднику+дате, по проекту+дате
    private async Task CreateTimeEntryIndexesAsync(CancellationToken cancellationToken)
    {
        var indexes = new[]
        {
            // Индекс по дате для отчётов за период
            new CreateIndexModel<TimeEntry>(
                Builders<TimeEntry>.IndexKeys.Ascending(x => x.Date),
                new CreateIndexOptions { Name = "ix_time_entries_date" }),

            // Составной индекс для фильтрации по сотруднику и дате
            new CreateIndexModel<TimeEntry>(
                Builders<TimeEntry>.IndexKeys
                    .Ascending(x => x.EmployeeId)
                    .Ascending(x => x.Date),
                new CreateIndexOptions { Name = "ix_time_entries_employee_date" }),

            // Составной индекс для фильтрации по проекту и дате
            new CreateIndexModel<TimeEntry>(
                Builders<TimeEntry>.IndexKeys
                    .Ascending(x => x.ProjectId)
                    .Ascending(x => x.Date),
                new CreateIndexOptions { Name = "ix_time_entries_project_date" })
        };

        await _db.TimeEntries.Indexes.CreateManyAsync(indexes, cancellationToken);
    }

    // Создаёт уникальный индекс для коллекции проектов. Гарантирует уникальность шифра проекта.
    private async Task CreateProjectIndexesAsync(CancellationToken cancellationToken)
    {
        var index = new CreateIndexModel<Project>(
            Builders<Project>.IndexKeys.Ascending(x => x.Code),
            new CreateIndexOptions
            {
                Name = "ux_projects_code",
                Unique = true
            });

        await _db.Projects.Indexes.CreateOneAsync(
            index,
            cancellationToken: cancellationToken);
    }

    // Создаёт уникальный составной индекс для закрытых периодов. Гарантирует один период на месяц.
    private async Task CreateClosedPeriodIndexesAsync(CancellationToken cancellationToken)
    {
        var index = new CreateIndexModel<ClosedPeriod>(
            Builders<ClosedPeriod>.IndexKeys
                .Ascending(x => x.Year)
                .Ascending(x => x.Month),
            new CreateIndexOptions
            {
                Name = "ux_closed_periods_year_month",
                Unique = true
            });

        await _db.ClosedPeriods.Indexes.CreateOneAsync(
            index,
            cancellationToken: cancellationToken);
    }
}
