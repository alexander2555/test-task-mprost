using Api.Application.Common;
using Api.Application.TimeEntries;
using Api.Contracts.Errors;
using Api.Contracts.TimeEntries;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// Контроллер для CRUD операций с записями табеля. Обрабатывает HTTP-запросы и преобразует исключения в ответы.
[ApiController]
[Route("api/time-entries")]
public sealed class TimeEntriesController : ControllerBase
{
    private readonly TimeEntryService _service;

    // Создаёт новый экземпляр контроллера с сервисом записей табеля
    public TimeEntriesController(TimeEntryService service)
    {
        _service = service;
    }

    // Создаёт новую запись табеля. Возвращает 201 Created с данными записи.
    [HttpPut]
    public async Task<ActionResult<TimeEntryResponse>> Create(
        CreateTimeEntryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var entry = await _service.CreateAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, entry);
        }
        catch (ApiException exception)
        {
            return ToErrorResult(exception);
        }
    }

    // Обновляет существующую запись табеля по ID. Возвращает 200 OK с обновлёнными данными.
    [HttpPost("{id}")]
    public async Task<ActionResult<TimeEntryResponse>> Update(
        string id,
        UpdateTimeEntryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var entry = await _service.UpdateAsync(
                id,
                request,
                cancellationToken);

            return Ok(entry);
        }
        catch (ApiException exception)
        {
            return ToErrorResult(exception);
        }
    }

    // Удаляет запись табеля по ID. Возвращает 204 No Content при успехе.
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        string id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _service.DeleteAsync(id, cancellationToken);
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
