using Api.Application.Common;
using Api.Contracts.TimeEntries;
using MongoDB.Bson;

namespace Api.Application.TimeEntries;

public static class TimeEntryQueryValidator
{
    public static TimeEntryQueryParameters Validate(TimeEntryQueryRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.Year < 1 || request.Year > 9998)
        {
            errors["year"] = ["Год должен быть в диапазоне от 1 до 9998."];
        }

        if (request.Month < 1 || request.Month > 12)
        {
            errors["month"] = ["Месяц должен быть в диапазоне от 1 до 12."];
        }

        if (request.Page <= 0)
        {
            errors["page"] = ["Номер страницы должен быть больше 0."];
        }

        if (request.PageSize <= 0)
        {
            errors["pageSize"] = ["Размер страницы должен быть больше 0."];
        }

        if (request.Page > 0 && request.PageSize > 0 &&
            (long)(request.Page - 1) * request.PageSize > int.MaxValue)
        {
            errors["page"] = ["Запрошенная страница находится за допустимым диапазоном пагинации."];
        }

        ObjectId? employeeId = null;
        if (!string.IsNullOrWhiteSpace(request.EmployeeId))
        {
            if (ObjectId.TryParse(request.EmployeeId, out var parsedEmployeeId))
            {
                employeeId = parsedEmployeeId;
            }
            else
            {
                errors["employeeId"] = ["Некорректный идентификатор сотрудника."];
            }
        }

        ObjectId? projectId = null;
        if (!string.IsNullOrWhiteSpace(request.ProjectId))
        {
            if (ObjectId.TryParse(request.ProjectId, out var parsedProjectId))
            {
                projectId = parsedProjectId;
            }
            else
            {
                errors["projectId"] = ["Некорректный идентификатор проекта."];
            }
        }

        if (errors.Count > 0)
        {
            throw new ApiException(
                "validation_error",
                "Проверьте введённые данные.",
                StatusCodes.Status400BadRequest,
                errors);
        }

        return new TimeEntryQueryParameters(
            request.Year,
            request.Month,
            employeeId,
            projectId,
            request.Page,
            request.PageSize);
    }
}

public sealed record TimeEntryQueryParameters(
    int Year,
    int Month,
    ObjectId? EmployeeId,
    ObjectId? ProjectId,
    int Page,
    int PageSize);
