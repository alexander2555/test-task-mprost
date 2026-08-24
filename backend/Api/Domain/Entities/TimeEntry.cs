using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Api.Domain.Entities;

public sealed class TimeEntry
{
    public ObjectId Id { get; set; }

    public ObjectId EmployeeId { get; set; }

    public ObjectId ProjectId { get; set; }

    [BsonDateOnlyOptions(BsonType.DateTime)]
    public DateOnly Date { get; set; }

    public decimal Hours { get; set; }

    public string Comment { get; set; } = string.Empty;

    public long Version { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
