using Api.Application.ReferenceData;
using Api.Contracts.ReferenceData;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// Контроллер для справочных данных (сотрудники и проекты). Предоставляет списки для выпадающих форм.
[ApiController]
public sealed class ReferenceDataController : ControllerBase
{
    private readonly ReferenceDataService _service;

    // Создаёт новый экземпляр контроллера с сервисом справочных данных
    public ReferenceDataController(ReferenceDataService service)
    {
        _service = service;
    }

    // Возвращает список сотрудников для справочника. Используется в выпадающих списках.
    [HttpGet("api/employees")]
    public async Task<ActionResult<IReadOnlyList<EmployeeReferenceItem>>> GetEmployees(
        CancellationToken cancellationToken)
    {
        var employees = await _service.GetEmployeesAsync(cancellationToken);
        return Ok(employees);
    }

    // Возвращает список проектов для справочника. Используется в выпадающих списках.
    [HttpGet("api/projects")]
    public async Task<ActionResult<IReadOnlyList<ProjectReferenceItem>>> GetProjects(
        CancellationToken cancellationToken)
    {
        var projects = await _service.GetProjectsAsync(cancellationToken);
        return Ok(projects);
    }
}
