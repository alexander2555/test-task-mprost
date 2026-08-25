using Api.Contracts.ReferenceData;
using Api.Domain.Entities;
using Api.Infrastructure.Mongo;
using MongoDB.Driver;

namespace Api.Application.ReferenceData;

// Сервис для загрузки справочных данных (сотрудники и проекты) для выпадающих списков
public sealed class ReferenceDataService
{
    private readonly MongoDbContext _db;

    public ReferenceDataService(MongoDbContext db)
    {
        _db = db;
    }

    // Получает список сотрудников, отсортированных по имени
    public async Task<IReadOnlyList<EmployeeReferenceItem>> GetEmployeesAsync(
        CancellationToken cancellationToken)
    {
        var employees = await _db.Employees
            .Find(Builders<Employee>.Filter.Empty)
            .SortBy(employee => employee.FullName)
            .ToListAsync(cancellationToken);

        return employees
            .Select(employee => new EmployeeReferenceItem
            {
                Id = employee.Id.ToString(),
                FullName = employee.FullName,
                Department = employee.Department
            })
            .ToArray();
    }

    // Получает список проектов, отсортированных по шифру
    public async Task<IReadOnlyList<ProjectReferenceItem>> GetProjectsAsync(
        CancellationToken cancellationToken)
    {
        var projects = await _db.Projects
            .Find(Builders<Project>.Filter.Empty)
            .SortBy(project => project.Code)
            .ToListAsync(cancellationToken);

        return projects
            .Select(project => new ProjectReferenceItem
            {
                Id = project.Id.ToString(),
                Code = project.Code,
                Name = project.Name
            })
            .ToArray();
    }
}
