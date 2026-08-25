using Api.Infrastructure.Mongo;

// Точка входа в приложение. Настраивает DI-контейнер, MongoDB, индексы и HTTP-эндпоинты.
var builder = WebApplication.CreateBuilder(args);

// Регистрация MVC-контроллеров для обработки HTTP-запросов
builder.Services.AddControllers();

// Настройка MongoDB: конвенции сериализации, подключения, контекст и инициализация индексов
builder.Services.AddMongoDb(builder.Configuration);

var app = builder.Build();

// Создание индексов в MongoDB при запуске приложения для оптимизации запросов
var indexInitializer = app.Services.GetRequiredService<MongoIndexInitializer>();
await indexInitializer.EnsureIndexesAsync();

// Подключение маршрутизации контроллеров
app.MapControllers();

// Эндпоинт проверки работоспособности приложения
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.Run();

// Позволяет использовать этот класс в интеграционных тестах
public partial class Program;
