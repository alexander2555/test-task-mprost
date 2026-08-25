using Api.Application.Common;
using Api.Application.Reports;
using Api.Contracts.Errors;
using Api.Contracts.Reports;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// Контроллер для генерации отчётов. Обрабатывает запросы на проектные отчёты и преобразует исключения в ответы.
[ApiController]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly ProjectReportService _service;

    // Создаёт новый экземпляр контроллера с сервисом проектных отчётов
    public ReportsController(ProjectReportService service)
    {
        _service = service;
    }

    // Генерирует проектный отчёт за указанный период (год и месяц). Возвращает 200 OK с данными отчёта.
    [HttpGet("projects")]
    public async Task<ActionResult<ProjectReportResponse>> GetProjects(
        [FromQuery] ProjectReportQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.GetAsync(request, cancellationToken));
        }
        catch (ApiException exception)
        {
            return StatusCode(exception.StatusCode, new ApiErrorResponse
            {
                Code = exception.Code,
                Message = exception.Message,
                Errors = exception.Errors
            });
        }
    }
}
