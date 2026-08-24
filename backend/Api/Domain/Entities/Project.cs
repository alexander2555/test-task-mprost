using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Api.Domain.Entities;

public sealed class Project
{
    public ObjectId Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal Budget { get; set; }

    [BsonDateOnlyOptions(BsonType.DateTime)]
    public DateOnly StartDate { get; set; }

    [BsonDateOnlyOptions(BsonType.DateTime)]
    public DateOnly? EndDate { get; set; }
}
