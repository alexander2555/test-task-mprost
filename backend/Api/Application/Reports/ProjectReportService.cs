using Api.Contracts.Reports;
using Api.Domain.Entities;
using Api.Infrastructure.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Api.Application.Reports;

// Сервис для генерации проектного отчёта с агрегацией данных по MongoDB pipeline.
// Вычисляет часы, стоимость, бюджет и проценты использования по проектам за указанный период.
public sealed class ProjectReportService
{
    private readonly MongoDbContext _db;

    // Создаёт новый экземпляр сервиса с доступом к базе данных
    public ProjectReportService(MongoDbContext db)
    {
        _db = db;
    }

    // Генерирует проектный отчёт за указанный год и месяц с агрегацией по MongoDB pipeline
    public async Task<ProjectReportResponse> GetAsync(
        ProjectReportQuery request,
        CancellationToken cancellationToken)
    {
        ProjectReportQueryValidator.Validate(request);

        var from = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonth = request.Month == 12
            ? new DateTime(request.Year + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            : new DateTime(request.Year, request.Month + 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var documents = await _db.TimeEntries
            .Aggregate<BsonDocument>(BuildPipeline(from, nextMonth))
            .ToListAsync(cancellationToken);

        if (documents.Count == 0)
        {
            return EmptyResponse();
        }

        var result = documents[0];
        var rows = result["rows"].AsBsonArray
            .Select(value => MapRow(value.AsBsonDocument))
            .ToArray();

        var totalsArray = result["totals"].AsBsonArray;
        var totals = totalsArray.Count == 0
            ? EmptyTotals()
            : MapTotals(totalsArray[0].AsBsonDocument);

        return new ProjectReportResponse
        {
            Rows = rows,
            Totals = totals
        };
    }

    // Строит MongoDB aggregation pipeline для расчёта проектного отчёта: фильтрация по дате,
    // join с сотрудниками и проектами, расчёт ставок и агрегация
    private static PipelineDefinition<TimeEntry, BsonDocument> BuildPipeline(
        DateTime from,
        DateTime nextMonth)
    {
        var stages = new[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                ["date"] = new BsonDocument
                {
                    ["$gte"] = from,
                    ["$lt"] = nextMonth
                }
            }),
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
                ["projectId"] = new BsonDocument("$first", "$projectId"),
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
                ["_id"] = "$projectId",
                ["hours"] = new BsonDocument("$sum", "$hours"),
                ["amount"] = new BsonDocument("$sum", "$amount")
            }),
            new BsonDocument("$lookup", new BsonDocument
            {
                ["from"] = "projects",
                ["localField"] = "_id",
                ["foreignField"] = "_id",
                ["as"] = "project"
            }),
            new BsonDocument("$unwind", "$project"),
            new BsonDocument("$set", new BsonDocument
            {
                ["projectId"] = "$_id",
                ["projectCode"] = "$project.code",
                ["projectName"] = "$project.name",
                ["budget"] = "$project.budget",
                ["percent"] = PercentExpression("$amount", "$project.budget")
            }),
            new BsonDocument("$set", new BsonDocument
            {
                ["overspent"] = ThresholdExpression("$percent", 100),
                ["risk"] = ThresholdExpression("$percent", 80)
            }),
            new BsonDocument("$sort", new BsonDocument("projectCode", 1)),
            new BsonDocument("$facet", new BsonDocument
            {
                ["rows"] = new BsonArray
                {
                    new BsonDocument("$project", new BsonDocument
                    {
                        ["_id"] = 0,
                        ["projectId"] = 1,
                        ["projectCode"] = 1,
                        ["projectName"] = 1,
                        ["hours"] = 1,
                        ["amount"] = 1,
                        ["budget"] = 1,
                        ["percent"] = 1,
                        ["overspent"] = 1,
                        ["risk"] = 1
                    })
                },
                ["totals"] = new BsonArray
                {
                    new BsonDocument("$group", new BsonDocument
                    {
                        ["_id"] = BsonNull.Value,
                        ["hours"] = new BsonDocument("$sum", "$hours"),
                        ["amount"] = new BsonDocument("$sum", "$amount"),
                        ["budget"] = new BsonDocument("$sum", "$budget")
                    }),
                    new BsonDocument("$set", new BsonDocument(
                        "percent",
                        PercentExpression("$amount", "$budget"))),
                    new BsonDocument("$set", new BsonDocument
                    {
                        ["overspent"] = ThresholdExpression("$percent", 100),
                        ["risk"] = ThresholdExpression("$percent", 80)
                    }),
                    new BsonDocument("$project", new BsonDocument
                    {
                        ["_id"] = 0,
                        ["hours"] = 1,
                        ["amount"] = 1,
                        ["budget"] = 1,
                        ["percent"] = 1,
                        ["overspent"] = 1,
                        ["risk"] = 1
                    })
                }
            })
        };

        return stages;
    }

    // Создаёт MongoDB выражение для расчёта процента от бюджета с защитой от деления на ноль
    private static BsonDocument PercentExpression(string amount, string budget)
    {
        return new BsonDocument("$cond", new BsonArray
        {
            new BsonDocument("$eq", new BsonArray { budget, 0 }),
            BsonNull.Value,
            new BsonDocument("$round", new BsonArray
            {
                new BsonDocument("$multiply", new BsonArray
                {
                    new BsonDocument("$divide", new BsonArray { amount, budget }),
                    100
                }),
                2
            })
        });
    }

    // Создаёт MongoDB выражение для проверки превышения порога (процента)
    private static BsonDocument ThresholdExpression(string percent, int threshold)
    {
        return new BsonDocument("$cond", new BsonArray
        {
            new BsonDocument("$eq", new BsonArray { percent, BsonNull.Value }),
            false,
            new BsonDocument("$gt", new BsonArray { percent, threshold })
        });
    }

    // Преобразует BSON документ в DTO строки отчёта
    private static ProjectReportRow MapRow(BsonDocument document)
    {
        return new ProjectReportRow
        {
            ProjectId = document["projectId"].AsObjectId.ToString(),
            ProjectCode = document["projectCode"].AsString,
            ProjectName = document["projectName"].AsString,
            Hours = ToDecimal(document["hours"]),
            Amount = ToDecimal(document["amount"]),
            Budget = ToDecimal(document["budget"]),
            Percent = ToNullableDecimal(document["percent"]),
            Overspent = document["overspent"].AsBoolean,
            Risk = document["risk"].AsBoolean
        };
    }

    // Преобразует BSON документ в DTO итогов отчёта
    private static ProjectReportTotals MapTotals(BsonDocument document)
    {
        return new ProjectReportTotals
        {
            Hours = ToDecimal(document["hours"]),
            Amount = ToDecimal(document["amount"]),
            Budget = ToDecimal(document["budget"]),
            Percent = ToNullableDecimal(document["percent"]),
            Overspent = document["overspent"].AsBoolean,
            Risk = document["risk"].AsBoolean
        };
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

        throw new InvalidOperationException($"Expected numeric BSON value, got {value.BsonType}.");
    }

    // Преобразует BSON значение в nullable decimal (null для BsonNull)
    private static decimal? ToNullableDecimal(BsonValue value) =>
        value.IsBsonNull ? null : ToDecimal(value);

    // Создаёт пустой ответ отчёта (когда нет данных за период)
    private static ProjectReportResponse EmptyResponse() => new()
    {
        Rows = [],
        Totals = EmptyTotals()
    };

    // Создаёт пустые итоги отчёта
    private static ProjectReportTotals EmptyTotals() => new()
    {
        Hours = 0,
        Amount = 0,
        Budget = 0,
        Percent = null,
        Overspent = false,
        Risk = false
    };
}
