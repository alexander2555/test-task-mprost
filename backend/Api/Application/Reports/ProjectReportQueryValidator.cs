using Api.Application.Common;
using Api.Contracts.Reports;

namespace Api.Application.Reports;

// Валидатор запроса проектного отчёта. Проверяет корректность года и месяца.
public static class ProjectReportQueryValidator
{
    // Валидирует параметры запроса отчёта: год (1-9998) и месяц (1-12)
    public static void Validate(ProjectReportQuery request)
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

        if (errors.Count > 0)
        {
            throw new ApiException(
                "validation_error",
                "Проверьте введённые данные.",
                StatusCodes.Status400BadRequest,
                errors);
        }
    }
}
