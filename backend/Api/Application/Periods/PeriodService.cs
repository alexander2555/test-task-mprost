using Api.Application.Common;
using Api.Contracts.Periods;
using Api.Domain.Entities;
using Api.Infrastructure.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Api.Application.Periods;

// Сервис для управления расчётными периодами. Закрывает и открывает периоды с защитой от конкурентных операций.
public sealed class PeriodService
{
    private readonly MongoDbContext _db;

    // Создаёт новый экземпляр сервиса с доступом к базе данных
    public PeriodService(MongoDbContext db)
    {
        _db = db;
    }

    // Закрывает расчётный период. Использует уникальный индекс для защиты от повторного закрытия.
    public async Task CloseAsync(
        PeriodRequest request,
        CancellationToken cancellationToken)
    {
        PeriodRequestValidator.Validate(request);

        var period = new ClosedPeriod
        {
            Id = ObjectId.GenerateNewId(),
            Year = request.Year,
            Month = request.Month,
            ClosedAtUtc = DateTime.UtcNow
        };

        try
        {
            await _db.ClosedPeriods.InsertOneAsync(
                period,
                cancellationToken: cancellationToken);
        }
        catch (MongoWriteException exception)
            when (exception.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new ApiException(
                "period_already_closed",
                "Период уже закрыт.",
                StatusCodes.Status409Conflict);
        }
    }

    // Открывает ранее закрытый расчётный период. Возвращает ошибку если период не был закрыт.
    public async Task OpenAsync(
        PeriodRequest request,
        CancellationToken cancellationToken)
    {
        PeriodRequestValidator.Validate(request);

        var result = await _db.ClosedPeriods.DeleteOneAsync(
            period => period.Year == request.Year &&
                      period.Month == request.Month,
            cancellationToken);

        if (result.DeletedCount == 0)
        {
            throw new ApiException(
                "period_not_closed",
                "Период не закрыт.",
                StatusCodes.Status409Conflict);
        }
    }
}
