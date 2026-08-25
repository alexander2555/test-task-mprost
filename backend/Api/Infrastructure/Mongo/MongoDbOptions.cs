namespace Api.Infrastructure.Mongo;

// Опции конфигурации MongoDB. Загружаются из секции "MongoDb" в appsettings.json.
public sealed class MongoDbOptions
{
    // Имя секции в конфигурации (appsettings.json), откуда загружаются опции
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; init; } = string.Empty;

    public string DatabaseName { get; init; } = string.Empty;
}
