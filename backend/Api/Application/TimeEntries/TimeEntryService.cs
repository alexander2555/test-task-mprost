using Api.Application.Common;
using Api.Contracts.TimeEntries;
using Api.Domain.Entities;
using Api.Infrastructure.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Api.Application.TimeEntries;

// Сервис для CRUD операций с записями табеля. Реализует бизнес-логику и валидацию.
public sealed class TimeEntryService
{
    private readonly MongoDbContext _db;

    // Создаёт новый экземпляр сервиса с доступом к базе данных
    public TimeEntryService(MongoDbContext db)
    {
        _db = db;
    }

    // Создаёт новую запись табеля с валидацией всех бизнес-правил
    public async Task<TimeEntryResponse> CreateAsync(
        CreateTimeEntryRequest request,
        CancellationToken cancellationToken)
    {
        TimeEntryRules.EnsureValidHours(request.Hours);

        var employeeId = ParseObjectId(request.EmployeeId, "employeeId");
        var projectId = ParseObjectId(request.ProjectId, "projectId");
        var date = RequireDate(request.Date);

        await EnsurePeriodOpenAsync(date, cancellationToken);

        var employee = await _db.Employees
            .Find(x => x.Id == employeeId)
            .FirstOrDefaultAsync(cancellationToken);

        if (employee is null)
        {
            throw new ApiException(
                "employee_not_found",
                "Сотрудник не найден.",
                StatusCodes.Status400BadRequest);
        }

        var project = await _db.Projects
            .Find(x => x.Id == projectId)
            .FirstOrDefaultAsync(cancellationToken);

        if (project is null)
        {
            throw new ApiException(
                "project_not_found",
                "Проект не найден.",
                StatusCodes.Status400BadRequest);
        }

        EnsureRateExists(employee, date);
        TimeEntryRules.EnsureDateWithinProject(project, date);

        var dailyHours = await GetDailyHoursAsync(
            employeeId,
            date,
            excludedEntryId: null,
            cancellationToken);

        TimeEntryRules.EnsureDailyHoursWithinLimit(dailyHours, request.Hours);

        var now = DateTime.UtcNow;
        var entry = new TimeEntry
        {
            Id = ObjectId.GenerateNewId(),
            EmployeeId = employeeId,
            ProjectId = projectId,
            Date = date,
            Hours = request.Hours,
            Comment = request.Comment?.Trim() ?? string.Empty,
            Version = 1,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        await _db.TimeEntries.InsertOneAsync(
            entry,
            cancellationToken: cancellationToken);

        return ToResponse(entry);
    }

    // Обновляет существующую запись табеля с проверкой версии и бизнес-правил
    public async Task<TimeEntryResponse> UpdateAsync(
        string id,
        UpdateTimeEntryRequest request,
        CancellationToken cancellationToken)
    {
        TimeEntryRules.EnsureValidHours(request.Hours);

        var entryId = ParseObjectId(id, "id");
        var employeeId = ParseObjectId(request.EmployeeId, "employeeId");
        var projectId = ParseObjectId(request.ProjectId, "projectId");
        var date = RequireDate(request.Date);

        var current = await _db.TimeEntries
            .Find(x => x.Id == entryId)
            .FirstOrDefaultAsync(cancellationToken);

        if (current is null)
        {
            throw TimeEntryNotFound();
        }

        TimeEntryRules.EnsureVersionMatches(current.Version, request.Version);

        // Запись из закрытого месяца нельзя переместить в другой месяц
        await EnsurePeriodOpenAsync(current.Date, cancellationToken);

        if (date != current.Date)
        {
            await EnsurePeriodOpenAsync(date, cancellationToken);
        }

        var employee = await _db.Employees
            .Find(x => x.Id == employeeId)
            .FirstOrDefaultAsync(cancellationToken);

        if (employee is null)
        {
            throw new ApiException(
                "employee_not_found",
                "Сотрудник не найден.",
                StatusCodes.Status400BadRequest);
        }

        var project = await _db.Projects
            .Find(x => x.Id == projectId)
            .FirstOrDefaultAsync(cancellationToken);

        if (project is null)
        {
            throw new ApiException(
                "project_not_found",
                "Проект не найден.",
                StatusCodes.Status400BadRequest);
        }

        EnsureRateExists(employee, date);
        TimeEntryRules.EnsureDateWithinProject(project, date);

        var dailyHours = await GetDailyHoursAsync(
            employeeId,
            date,
            entryId,
            cancellationToken);

        TimeEntryRules.EnsureDailyHoursWithinLimit(dailyHours, request.Hours);

        var updated = new TimeEntry
        {
            Id = current.Id,
            EmployeeId = employeeId,
            ProjectId = projectId,
            Date = date,
            Hours = request.Hours,
            Comment = request.Comment?.Trim() ?? string.Empty,
            Version = current.Version + 1,
            CreatedAtUtc = current.CreatedAtUtc,
            UpdatedAtUtc = DateTime.UtcNow
        };

        var updateFilter =
            Builders<TimeEntry>.Filter.Eq(x => x.Id, entryId) &
            Builders<TimeEntry>.Filter.Eq(x => x.Version, request.Version);

        var result = await _db.TimeEntries.ReplaceOneAsync(
            updateFilter,
            updated,
            cancellationToken: cancellationToken);

        if (result.ModifiedCount == 0)
        {
            var stillExists = await _db.TimeEntries
                .Find(x => x.Id == entryId)
                .AnyAsync(cancellationToken);

            if (!stillExists)
            {
                throw TimeEntryNotFound();
            }

            throw new ApiException(
                "time_entry_version_conflict",
                "Запись была изменена другим пользователем. Обновите данные и повторите попытку.",
                StatusCodes.Status409Conflict);
        }

        return ToResponse(updated);
    }

    // Удаляет запись табеля с проверкой, что период не закрыт
    public async Task DeleteAsync(
        string id,
        CancellationToken cancellationToken)
    {
        var entryId = ParseObjectId(id, "id");

        var current = await _db.TimeEntries
            .Find(x => x.Id == entryId)
            .FirstOrDefaultAsync(cancellationToken);

        if (current is null)
        {
            throw TimeEntryNotFound();
        }

        await EnsurePeriodOpenAsync(current.Date, cancellationToken);

        var result = await _db.TimeEntries.DeleteOneAsync(
            x => x.Id == entryId,
            cancellationToken);

        if (result.DeletedCount == 0)
        {
            throw TimeEntryNotFound();
        }
    }

    // Проверяет, что период для указанной даты не закрыт
    private async Task EnsurePeriodOpenAsync(
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var isClosed = await _db.ClosedPeriods
            .Find(x => x.Year == date.Year && x.Month == date.Month)
            .AnyAsync(cancellationToken);

        TimeEntryRules.EnsurePeriodOpen(isClosed);
    }

    // Вычисляет суммарные часы сотрудника за указанную дату (исключая конкретную запись при обновлении)
    private async Task<decimal> GetDailyHoursAsync(
        ObjectId employeeId,
        DateOnly date,
        ObjectId? excludedEntryId,
        CancellationToken cancellationToken)
    {
        var filter =
            Builders<TimeEntry>.Filter.Eq(x => x.EmployeeId, employeeId) &
            Builders<TimeEntry>.Filter.Eq(x => x.Date, date);

        if (excludedEntryId.HasValue)
        {
            filter &= Builders<TimeEntry>.Filter.Ne(
                x => x.Id,
                excludedEntryId.Value);
        }

        var hours = await _db.TimeEntries
            .Find(filter)
            .Project(x => x.Hours)
            .ToListAsync(cancellationToken);

        return hours.Sum();
    }

    // Проверяет, что для сотрудника задана ставка на указанную дату
    private static void EnsureRateExists(Employee employee, DateOnly date)
    {
        if (RateResolver.Resolve(employee.Rates, date) is null)
        {
            throw new ApiException(
                "employee_rate_not_found",
                "На дату записи для сотрудника не задана действующая ставка.",
                StatusCodes.Status400BadRequest);
        }
    }

    // Парсит строку в ObjectId с генерацией ошибки валидации при неудаче
    private static ObjectId ParseObjectId(string? value, string field)
    {
        if (!ObjectId.TryParse(value, out var id))
        {
            throw new ApiException(
                "validation_error",
                "Проверьте введённые данные.",
                StatusCodes.Status400BadRequest,
                new Dictionary<string, string[]>
                {
                    [field] = ["Некорректный идентификатор."]
                });
        }

        return id;
    }

    // Проверяет, что дата указана, иначе генерирует ошибку валидации
    private static DateOnly RequireDate(DateOnly? date)
    {
        if (date.HasValue)
        {
            return date.Value;
        }

        throw new ApiException(
            "validation_error",
            "Проверьте введённые данные.",
            StatusCodes.Status400BadRequest,
            new Dictionary<string, string[]>
            {
                ["date"] = ["Дата обязательна."]
            });
    }

    // Создаёт стандартное исключение для случая, когда запись табеля не найдена
    private static ApiException TimeEntryNotFound()
    {
        return new ApiException(
            "time_entry_not_found",
            "Запись табеля не найдена.",
            StatusCodes.Status404NotFound);
    }

    // Преобразует сущность TimeEntry в DTO TimeEntryResponse
    private static TimeEntryResponse ToResponse(TimeEntry entry)
    {
        return new TimeEntryResponse
        {
            Id = entry.Id.ToString(),
            EmployeeId = entry.EmployeeId.ToString(),
            ProjectId = entry.ProjectId.ToString(),
            Date = entry.Date,
            Hours = entry.Hours,
            Comment = entry.Comment,
            Version = entry.Version,
            CreatedAtUtc = entry.CreatedAtUtc,
            UpdatedAtUtc = entry.UpdatedAtUtc
        };
    }
}
