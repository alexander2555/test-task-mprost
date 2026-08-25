using Api.Application.Common;
using Api.Application.Periods;
using Api.Contracts.Errors;
using Api.Contracts.Periods;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// Контроллер для управления расчётными периодами. Обрабатывает закрытие и открытие периодов.
[ApiController]
[Route("api/periods")]
public sealed class PeriodsController : ControllerBase
{
    private readonly PeriodService _service;

    // Создаёт новый экземпляр контроллера с сервисом периодов
    public PeriodsController(PeriodService service)
    {
        _service = service;
    }

    // Закрывает расчётный период. После закрытия записи за этот период нельзя изменять. Возвращает 204 No Content.
    [HttpPost("close")]
    public async Task<IActionResult> Close(
        PeriodRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _service.CloseAsync(request, cancellationToken);
            return NoContent();
        }
        catch (ApiException exception)
        {
            return ToErrorResult(exception);
        }
    }

    // Открывает ранее закрытый расчётный период. Возвращает 204 No Content.
    [HttpPost("open")]
    public async Task<IActionResult> Open(
        PeriodRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _service.OpenAsync(request, cancellationToken);
            return NoContent();
        }
        catch (ApiException exception)
        {
            return ToErrorResult(exception);
        }
    }

    // Преобразует ApiException в HTTP-ответ с соответствующим статусом и телом ошибки
    private ObjectResult ToErrorResult(ApiException exception)
    {
        return StatusCode(
            exception.StatusCode,
            new ApiErrorResponse
            {
                Code = exception.Code,
                Message = exception.Message,
                Errors = exception.Errors
            });
    }
}
