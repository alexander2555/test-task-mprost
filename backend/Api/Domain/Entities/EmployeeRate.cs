using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Api.Domain.Entities;

public sealed class EmployeeRate
{
    [BsonDateOnlyOptions(BsonType.DateTime)]
    public DateOnly From { get; set; }

    public decimal Value { get; set; }
}
