using MongoDB.Bson;

namespace Api.Domain.Entities;

public sealed class Employee
{
    public ObjectId Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public List<EmployeeRate> Rates { get; set; } = [];
}
