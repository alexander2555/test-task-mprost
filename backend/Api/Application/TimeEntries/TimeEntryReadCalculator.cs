using Api.Application.Common;
using Api.Domain.Entities;

namespace Api.Application.TimeEntries;

public static class TimeEntryReadCalculator
{
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

    public static decimal CalculateAmount(
        decimal hours,
        decimal rate)
    {
        return Math.Round(
            hours * rate,
            2,
            MidpointRounding.AwayFromZero);
    }

    public static bool IsOvertime(decimal dailyHours)
    {
        return dailyHours > 12m;
    }

    public static ApiException DataInconsistent(string message)
    {
        return new ApiException(
            "time_entry_data_inconsistent",
            message,
            StatusCodes.Status500InternalServerError);
    }
}
