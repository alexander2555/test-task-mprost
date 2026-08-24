using MongoDB.Bson;

namespace Api.Domain.Entities;

public sealed class ClosedPeriod
{
    public ObjectId Id { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public DateTime ClosedAtUtc { get; set; }
}
