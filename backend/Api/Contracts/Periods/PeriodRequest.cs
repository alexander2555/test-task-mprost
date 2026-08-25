namespace Api.Contracts.Periods;

// Запрос на закрытие или открытие расчётного периода
public sealed class PeriodRequest
{
    // Год периода (1-9999)
    public int Year { get; init; }

    // Месяц периода (1-12)
    public int Month { get; init; }
}
