using MongoDB.Bson.Serialization.Conventions;

namespace Api.Infrastructure.Mongo;

public static class MongoConventions
{
    private static int _registered;

    public static void Register()
    {
        if (Interlocked.Exchange(ref _registered, 1) != 0)
        {
            return;
        }

        var conventions = new ConventionPack
        {
            new CamelCaseElementNameConvention()
        };

        ConventionRegistry.Register(
            "application-conventions",
            conventions,
            _ => true);
    }
}
