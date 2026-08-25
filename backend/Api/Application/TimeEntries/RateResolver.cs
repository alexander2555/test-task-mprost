using Api.Domain.Entities;

namespace Api.Application.TimeEntries;

// Резолвер ставок сотрудника. Определяет актуальную ставку на конкретную дату.
public static class RateResolver
{
    // Определяет актуальную ставку сотрудника на указанную дату.
    // Возвращает ставку с максимальной датой From, которая <= указанной дате.
    public static EmployeeRate? Resolve(
        IReadOnlyCollection<EmployeeRate> rates,
        DateOnly date)
    {
        return rates
            .Where(rate => rate.From <= date)
            .OrderByDescending(rate => rate.From)
            .FirstOrDefault();
    }
}
