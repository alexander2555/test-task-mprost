using Api.Application.Common;
using Api.Domain.Entities;

namespace Api.Application.TimeEntries;

// Калькулятор для чтения записей табеля. Вычисляет ставки, стоимость и переработки.
public static class TimeEntryReadCalculator
{
    // Определяет актуальную ставку сотрудника на дату с проверкой её наличия
    public static decimal ResolveRate(Employee employee, DateOnly date)
    {
        var rate = RateResolver.Resolve(employee.Rates, date);

        if (rate is null)
        {
            throw DataInconsistent(
                "Для записи табеля отсутствует ставка сотрудника на дату работы.");
        }

        return rate.Value;
    }

    // Вычисляет стоимость по часам и ставке с округлением до 2 знаков
    public static decimal CalculateAmount(
        decimal hours,
        decimal rate)
    {
        return Math.Round(
            hours * rate,
            2,
            MidpointRounding.ToEven);
    }

    // Определяет переработку (true если суммарные часы за день > 12)
    public static bool IsOvertime(decimal dailyHours)
    {
        return dailyHours > 12m;
    }

    // Создаёт исключение о несогласованности данных (отсутствие ставки или ссылки)
    public static ApiException DataInconsistent(string message)
    {
        return new ApiException(
            "time_entry_data_inconsistent",
            message,
            StatusCodes.Status500InternalServerError);
    }
}
