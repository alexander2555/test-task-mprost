using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Api.Infrastructure.Mongo;

// Расширение для IServiceCollection для регистрации служб MongoDB в DI-контейнере.
// Обеспечивает правильную настройку и жизненный цикл всех компонентов MongoDB.
public static class MongoServiceCollectionExtensions
{
    // Регистрирует все необходимые службы MongoDB в DI-контейнере
    public static IServiceCollection AddMongoDb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Регистрация конвенций сериализации (camelCase для полей)
        MongoConventions.Register();

        // Регистрация и валидация опций конфигурации MongoDB
        services
            .AddOptions<MongoDbOptions>()
            .Bind(configuration.GetSection(MongoDbOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ConnectionString),
                "MongoDb:ConnectionString is required.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.DatabaseName),
                "MongoDb:DatabaseName is required.")
            .ValidateOnStart();

        // Регистрация MongoClient как singleton (thread-safe)
        services.AddSingleton<IMongoClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<MongoDbOptions>>().Value;
            return new MongoClient(options.ConnectionString);
        });

        // Регистрация IMongoDatabase как singleton
        services.AddSingleton<IMongoDatabase>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<MongoDbOptions>>().Value;
            var client = provider.GetRequiredService<IMongoClient>();
            return client.GetDatabase(options.DatabaseName);
        });

        // Регистрация контекста базы данных как singleton
        services.AddSingleton<MongoDbContext>();

        // Регистрация инициализатора индексов как singleton
        services.AddSingleton<MongoIndexInitializer>();

        return services;
    }
}
