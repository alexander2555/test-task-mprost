using Api.Application.Common;
using Api.Contracts.Periods;

namespace Api.Application.Periods;

// Валидатор запроса периода. Проверяет корректность года и месяца.
public static class PeriodRequestValidator
{
    // Валидирует параметры запроса периода: год (1-9999) и месяц (1-12)
    public static void Validate(PeriodRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.Year < 1 || request.Year > 9999)
        {
            errors["year"] = ["Год должен быть в диапазоне от 1 до 9999."];
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
