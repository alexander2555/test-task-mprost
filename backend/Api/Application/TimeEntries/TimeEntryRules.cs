using Api.Application.Common;
using Api.Domain.Entities;

namespace Api.Application.TimeEntries;

// Бизнес-правила валидации для записей табеля. Проверяют часы, даты, периоды и версии.
public static class TimeEntryRules
{
    // Проверяет валидность количества часов: > 0, <= 24, кратно 0.5
    public static void EnsureValidHours(decimal hours)
    {
        if (hours <= 0)
        {
            throw ValidationError("hours", "Количество часов должно быть больше 0.");
        }

        if (hours > 24)
        {
            throw ValidationError("hours", "Количество часов в одной записи не может превышать 24.");
        }

        if (hours % 0.5m != 0)
        {
            throw ValidationError("hours", "Количество часов должно быть кратно 0,5.");
        }
    }

    // Проверяет, что дата записи находится в периоде проекта
    public static void EnsureDateWithinProject(Project project, DateOnly date)
    {
        if (date < project.StartDate ||
            (project.EndDate.HasValue && date > project.EndDate.Value))
        {
            throw new ApiException(
                "time_entry_outside_project_period",
                "Дата записи должна находиться в периоде проекта.",
                StatusCodes.Status400BadRequest);
        }
    }

    // Проверяет, что суммарные часы за день не превышают 24
    public static void EnsureDailyHoursWithinLimit(
        decimal existingHours,
        decimal requestedHours)
    {
        if (existingHours + requestedHours > 24)
        {
            throw new ApiException(
                "daily_hours_limit_exceeded",
                "Суммарное количество часов сотрудника за день не может превышать 24.",
                StatusCodes.Status400BadRequest);
        }
    }

    // Проверяет, что период не закрыт. Закрытые периоды нельзя изменять.
    public static void EnsurePeriodOpen(bool isClosed)
    {
        if (isClosed)
        {
            throw new ApiException(
                "period_closed",
                "Закрытый период нельзя изменять.",
                StatusCodes.Status409Conflict);
        }
    }

    // Проверяет совпадение версии для оптимистической блокировки
    public static void EnsureVersionMatches(long currentVersion, long requestedVersion)
    {
        if (currentVersion != requestedVersion)
        {
            throw new ApiException(
                "time_entry_version_conflict",
                "Запись была изменена другим пользователем. Обновите данные и повторите попытку.",
                StatusCodes.Status409Conflict);
        }
    }

    // Создаёт исключение валидации для конкретного поля
    private static ApiException ValidationError(string field, string message)
    {
        return new ApiException(
            "validation_error",
            "Проверьте введённые данные.",
            StatusCodes.Status400BadRequest,
            new Dictionary<string, string[]>
            {
                [field] = [message]
            });
    }
}
