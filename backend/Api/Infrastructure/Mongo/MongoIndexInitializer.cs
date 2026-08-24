using Api.Domain.Entities;
using MongoDB.Driver;

namespace Api.Infrastructure.Mongo;

public sealed class MongoIndexInitializer
{
    private readonly MongoDbContext _db;

    public MongoIndexInitializer(MongoDbContext db)
    {
        _db = db;
    }

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
    {
        await CreateTimeEntryIndexesAsync(cancellationToken);
        await CreateProjectIndexesAsync(cancellationToken);
        await CreateClosedPeriodIndexesAsync(cancellationToken);
    }

    private async Task CreateTimeEntryIndexesAsync(CancellationToken cancellationToken)
    {
        var indexes = new[]
        {
            new CreateIndexModel<TimeEntry>(
                Builders<TimeEntry>.IndexKeys.Ascending(x => x.Date),
                new CreateIndexOptions { Name = "ix_time_entries_date" }),

            new CreateIndexModel<TimeEntry>(
                Builders<TimeEntry>.IndexKeys
                    .Ascending(x => x.EmployeeId)
                    .Ascending(x => x.Date),
                new CreateIndexOptions { Name = "ix_time_entries_employee_date" }),

            new CreateIndexModel<TimeEntry>(
                Builders<TimeEntry>.IndexKeys
                    .Ascending(x => x.ProjectId)
                    .Ascending(x => x.Date),
                new CreateIndexOptions { Name = "ix_time_entries_project_date" })
        };

        await _db.TimeEntries.Indexes.CreateManyAsync(indexes, cancellationToken);
    }

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
