using Api.Contracts.TimeEntries;
using Api.Domain.Entities;
using Api.Infrastructure.Mongo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Api.Application.TimeEntries;

// Сервис для чтения записей табеля с пагинацией, батч-загрузкой связанных данных и вычислением ставок и переработок
public sealed class TimeEntryQueryService
{
    private readonly MongoDbContext _db;

    public TimeEntryQueryService(MongoDbContext db)
    {
        _db = db;
    }

    // Получает список записей табеля с пагинацией, вычислением ставок, стоимости и переработок
    public async Task<TimeEntryListResponse> GetAsync(
        TimeEntryQueryRequest request,
        CancellationToken cancellationToken)
    {
        var query = TimeEntryQueryValidator.Validate(request);
        var filter = BuildFilter(query);

        var totalCount = await _db.TimeEntries.CountDocumentsAsync(
            filter,
            cancellationToken: cancellationToken);

        var totals = await LoadTotalsAsync(
            query,
            cancellationToken);

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
                TotalHours = totals.Hours,
                TotalAmount = totals.Amount
            };
        }

        var employees = await LoadEmployeesAsync(
            entries.Select(entry => entry.EmployeeId),
            cancellationToken);

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
            TotalHours = totals.Hours,
            TotalAmount = totals.Amount
        };
    }

    // Строит фильтр MongoDB по периоду, сотруднику и проекту
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

    // Вычисляет итоговые часы и стоимость с помощью MongoDB aggregation pipeline
    private async Task<TimeEntryTotals> LoadTotalsAsync(
        TimeEntryQueryParameters query,
        CancellationToken cancellationToken)
    {
        var from = new DateTime(query.Year, query.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonth = query.Month == 12
            ? new DateTime(query.Year + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            : new DateTime(query.Year, query.Month + 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var match = new BsonDocument
        {
            ["date"] = new BsonDocument
            {
                ["$gte"] = from,
                ["$lt"] = nextMonth
            }
        };

        if (query.EmployeeId.HasValue)
        {
            match["employeeId"] = query.EmployeeId.Value;
        }

        if (query.ProjectId.HasValue)
        {
            match["projectId"] = query.ProjectId.Value;
        }

        var pipeline = new[]
        {
            new BsonDocument("$match", match),
            new BsonDocument("$lookup", new BsonDocument
            {
                ["from"] = "employees",
                ["localField"] = "employeeId",
                ["foreignField"] = "_id",
                ["as"] = "employee"
            }),
            new BsonDocument("$unwind", "$employee"),
            new BsonDocument("$unwind", "$employee.rates"),
            new BsonDocument("$match", new BsonDocument(
                "$expr",
                new BsonDocument("$lte", new BsonArray
                {
                    "$employee.rates.from",
                    "$date"
                }))),
            new BsonDocument("$sort", new BsonDocument
            {
                ["_id"] = 1,
                ["employee.rates.from"] = -1
            }),
            new BsonDocument("$group", new BsonDocument
            {
                ["_id"] = "$_id",
                ["hours"] = new BsonDocument("$first", "$hours"),
                ["rate"] = new BsonDocument("$first", "$employee.rates.value")
            }),
            new BsonDocument("$set", new BsonDocument(
                "amount",
                new BsonDocument("$round", new BsonArray
                {
                    new BsonDocument("$multiply", new BsonArray { "$hours", "$rate" }),
                    2
                }))),
            new BsonDocument("$group", new BsonDocument
            {
                ["_id"] = BsonNull.Value,
                ["hours"] = new BsonDocument("$sum", "$hours"),
                ["amount"] = new BsonDocument("$sum", "$amount")
            })
        };

        var documents = await _db.TimeEntries
            .Aggregate<BsonDocument>(pipeline)
            .ToListAsync(cancellationToken);

        if (documents.Count == 0)
        {
            return new TimeEntryTotals(0m, 0m);
        }

        var document = documents[0];

        return new TimeEntryTotals(
            ToDecimal(document["hours"]),
            ToDecimal(document["amount"]));
    }

    // Батч-загружает сотрудников по списку ID для избежания N+1 запросов
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

    // Батч-загружает проекты по списку ID для избежания N+1 запросов
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

    // Загружает суммарные часы по сотрудникам и датам для расчёта переработок
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

    // Преобразует BSON значение в decimal с поддержкой различных числовых типов
    private static decimal ToDecimal(BsonValue value)
    {
        if (value.IsDecimal128)
        {
            return Decimal128.ToDecimal(value.AsDecimal128);
        }

        if (value.IsInt32) return value.AsInt32;
        if (value.IsInt64) return value.AsInt64;
        if (value.IsDouble) return Convert.ToDecimal(value.AsDouble);

        throw new InvalidOperationException(
            $"Expected numeric BSON value, got {value.BsonType}.");
    }

    // Вспомогательный класс для проекции при загрузке ежедневных часов
    private sealed class TimeEntryDailySource
    {
        public ObjectId EmployeeId { get; init; }

        [BsonDateOnlyOptions(BsonType.DateTime)]
        public DateOnly Date { get; init; }

        public decimal Hours { get; init; }
    }

    // Итоговые суммы часов и стоимости
    private readonly record struct TimeEntryTotals(
        decimal Hours,
        decimal Amount);

    // Ключ для группировки по сотруднику и дате
    private readonly record struct EmployeeDateKey(
        ObjectId EmployeeId,
        DateOnly Date);
}
