using Api.Contracts.TimeEntries;
using Api.Domain.Entities;
using Api.Infrastructure.Mongo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Api.Application.TimeEntries;

public sealed class TimeEntryQueryService
{
    private readonly MongoDbContext _db;

    public TimeEntryQueryService(MongoDbContext db)
    {
        _db = db;
    }

    public async Task<TimeEntryListResponse> GetAsync(
        TimeEntryQueryRequest request,
        CancellationToken cancellationToken)
    {
        var query = TimeEntryQueryValidator.Validate(request);
        var filter = BuildFilter(query);

        var totalCount = await _db.TimeEntries.CountDocumentsAsync(
            filter,
            cancellationToken: cancellationToken);

        // Для итогов нужны только EmployeeId, Date и Hours за отфильтрованный месяц.
        // Детальные записи при этом всё равно загружаются постранично.
        var totalSources = await _db.TimeEntries
            .Find(filter)
            .Project(entry => new TimeEntryCostSource
            {
                EmployeeId = entry.EmployeeId,
                Date = entry.Date,
                Hours = entry.Hours
            })
            .ToListAsync(cancellationToken);

        var employees = await LoadEmployeesAsync(
            totalSources.Select(source => source.EmployeeId),
            cancellationToken);

        var totalHours = totalSources.Sum(source => source.Hours);
        var totalAmount = 0m;

        foreach (var source in totalSources)
        {
            if (!employees.TryGetValue(source.EmployeeId, out var employee))
            {
                throw TimeEntryReadCalculator.DataInconsistent(
                    "Для записи табеля не найден сотрудник.");
            }

            var rate = TimeEntryReadCalculator.ResolveRate(employee, source.Date);
            totalAmount += TimeEntryReadCalculator.CalculateAmount(source.Hours, rate);
        }

        totalAmount = Math.Round(
            totalAmount,
            2,
            MidpointRounding.AwayFromZero);

        var skip = checked((query.Page - 1) * query.PageSize);

        var entries = await _db.TimeEntries
            .Find(filter)
            .SortByDescending(entry => entry.Date)
            .ThenByDescending(entry => entry.CreatedAtUtc)
            .Skip(skip)
            .Limit(query.PageSize)
            .ToListAsync(cancellationToken);

        if (entries.Count == 0)
        {
            return new TimeEntryListResponse
            {
                Items = [],
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                TotalHours = totalHours,
                TotalAmount = totalAmount
            };
        }

        var pageEmployeeIds = entries
            .Select(entry => entry.EmployeeId)
            .Distinct()
            .ToArray();

        // Сотрудники уже могли быть загружены для totals. Дополняем словарь,
        // если filtered dataset пуст/изменился между запросами.
        var missingEmployeeIds = pageEmployeeIds
            .Where(id => !employees.ContainsKey(id))
            .ToArray();

        if (missingEmployeeIds.Length > 0)
        {
            var additionalEmployees = await LoadEmployeesAsync(
                missingEmployeeIds,
                cancellationToken);

            foreach (var pair in additionalEmployees)
            {
                employees[pair.Key] = pair.Value;
            }
        }

        var projects = await LoadProjectsAsync(
            entries.Select(entry => entry.ProjectId),
            cancellationToken);

        var dailyHours = await LoadDailyHoursAsync(
            entries,
            cancellationToken);

        var items = new List<TimeEntryListItem>(entries.Count);

        foreach (var entry in entries)
        {
            if (!employees.TryGetValue(entry.EmployeeId, out var employee))
            {
                throw TimeEntryReadCalculator.DataInconsistent(
                    "Для записи табеля не найден сотрудник.");
            }

            if (!projects.TryGetValue(entry.ProjectId, out var project))
            {
                throw TimeEntryReadCalculator.DataInconsistent(
                    "Для записи табеля не найден проект.");
            }

            var rate = TimeEntryReadCalculator.ResolveRate(employee, entry.Date);
            var amount = TimeEntryReadCalculator.CalculateAmount(entry.Hours, rate);

            var key = new EmployeeDateKey(entry.EmployeeId, entry.Date);
            dailyHours.TryGetValue(key, out var hoursForDay);

            items.Add(new TimeEntryListItem
            {
                Id = entry.Id.ToString(),
                EmployeeId = entry.EmployeeId.ToString(),
                EmployeeName = employee.FullName,
                ProjectId = entry.ProjectId.ToString(),
                ProjectCode = project.Code,
                Date = entry.Date,
                Hours = entry.Hours,
                Rate = rate,
                Amount = amount,
                Overtime = TimeEntryReadCalculator.IsOvertime(hoursForDay),
                Comment = entry.Comment,
                Version = entry.Version
            });
        }

        return new TimeEntryListResponse
        {
            Items = items,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalHours = totalHours,
            TotalAmount = totalAmount
        };
    }

    private static FilterDefinition<TimeEntry> BuildFilter(
        TimeEntryQueryParameters query)
    {
        var from = new DateOnly(query.Year, query.Month, 1);
        var to = query.Month == 12
            ? new DateOnly(query.Year + 1, 1, 1)
            : new DateOnly(query.Year, query.Month + 1, 1);

        var filter =
            Builders<TimeEntry>.Filter.Gte(entry => entry.Date, from) &
            Builders<TimeEntry>.Filter.Lt(entry => entry.Date, to);

        if (query.EmployeeId.HasValue)
        {
            filter &= Builders<TimeEntry>.Filter.Eq(
                entry => entry.EmployeeId,
                query.EmployeeId.Value);
        }

        if (query.ProjectId.HasValue)
        {
            filter &= Builders<TimeEntry>.Filter.Eq(
                entry => entry.ProjectId,
                query.ProjectId.Value);
        }

        return filter;
    }

    private async Task<Dictionary<ObjectId, Employee>> LoadEmployeesAsync(
        IEnumerable<ObjectId> ids,
        CancellationToken cancellationToken)
    {
        var distinctIds = ids.Distinct().ToArray();

        if (distinctIds.Length == 0)
        {
            return [];
        }

        var employees = await _db.Employees
            .Find(Builders<Employee>.Filter.In(employee => employee.Id, distinctIds))
            .ToListAsync(cancellationToken);

        return employees.ToDictionary(employee => employee.Id);
    }

    private async Task<Dictionary<ObjectId, Project>> LoadProjectsAsync(
        IEnumerable<ObjectId> ids,
        CancellationToken cancellationToken)
    {
        var distinctIds = ids.Distinct().ToArray();

        if (distinctIds.Length == 0)
        {
            return [];
        }

        var projects = await _db.Projects
            .Find(Builders<Project>.Filter.In(project => project.Id, distinctIds))
            .ToListAsync(cancellationToken);

        return projects.ToDictionary(project => project.Id);
    }

    private async Task<Dictionary<EmployeeDateKey, decimal>> LoadDailyHoursAsync(
        IReadOnlyCollection<TimeEntry> pageEntries,
        CancellationToken cancellationToken)
    {
        var pairs = pageEntries
            .Select(entry => new EmployeeDateKey(entry.EmployeeId, entry.Date))
            .Distinct()
            .ToArray();

        var pairFilters = pairs
            .Select(pair =>
                Builders<TimeEntry>.Filter.Eq(entry => entry.EmployeeId, pair.EmployeeId) &
                Builders<TimeEntry>.Filter.Eq(entry => entry.Date, pair.Date))
            .ToArray();

        var filter = Builders<TimeEntry>.Filter.Or(pairFilters);

        var entries = await _db.TimeEntries
            .Find(filter)
            .Project(entry => new TimeEntryDailySource
            {
                EmployeeId = entry.EmployeeId,
                Date = entry.Date,
                Hours = entry.Hours
            })
            .ToListAsync(cancellationToken);

        return entries
            .GroupBy(entry => new EmployeeDateKey(entry.EmployeeId, entry.Date))
            .ToDictionary(
                group => group.Key,
                group => group.Sum(entry => entry.Hours));
    }

    private sealed class TimeEntryCostSource
    {
        public ObjectId EmployeeId { get; init; }

        [BsonDateOnlyOptions(BsonType.DateTime)]
        public DateOnly Date { get; init; }

        public decimal Hours { get; init; }
    }

    private sealed class TimeEntryDailySource
    {
        public ObjectId EmployeeId { get; init; }

        [BsonDateOnlyOptions(BsonType.DateTime)]
        public DateOnly Date { get; init; }

        public decimal Hours { get; init; }
    }

    private readonly record struct EmployeeDateKey(
        ObjectId EmployeeId,
        DateOnly Date);
}
