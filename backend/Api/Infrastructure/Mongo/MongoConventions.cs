using MongoDB.Bson.Serialization.Conventions;

namespace Api.Infrastructure.Mongo;

// Конвенции сериализации MongoDB. Обеспечивает camelCase-именование полей в документах.
// Регистрируется только один раз благодаря использованию Interlocked для thread-safety.
public static class MongoConventions
{
    // Флаг для предотвращения повторной регистрации конвенций
    private static int _registered;

    // Регистрирует конвенции сериализации. Использует Interlocked для thread-safety.
    public static void Register()
    {
        // Если конвенции уже зарегистрированы, выходим
        if (Interlocked.Exchange(ref _registered, 1) != 0)
        {
            return;
        }

        // Пакет конвенций: camelCase для имён полей
        var conventions = new ConventionPack
        {
            new CamelCaseElementNameConvention()
        };

        // Регистрация конвенций для всех типов
        ConventionRegistry.Register(
            "application-conventions",
            conventions,
            _ => true);
    }
}
