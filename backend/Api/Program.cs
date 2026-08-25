using Api.Application.TimeEntries;
using Api.Contracts.Errors;
using Api.Infrastructure.Mongo;
using Microsoft.AspNetCore.Mvc;

// Точка входа в приложение. Настраивает DI-контейнер, MongoDB, индексы и HTTP-эндпоинты.
var builder = WebApplication.CreateBuilder(args);

// Регистрация MVC-контроллеров и единого контракта ошибок model binding / validation.
builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(pair => pair.Value?.Errors.Count > 0)
                .ToDictionary(
                    pair => ToCamelCase(pair.Key),
                    pair => pair.Value!.Errors
                        .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                            ? "Некорректное значение."
                            : error.ErrorMessage)
                        .ToArray());

            return new BadRequestObjectResult(
                new ApiErrorResponse
                {
                    Code = "validation_error",
                    Message = "Проверьте введённые данные.",
                    Errors = errors
                });
        };
    });

// Настройка MongoDB: конвенции сериализации, подключения, контекст и инициализация индексов.
builder.Services.AddMongoDb(builder.Configuration);

// Application service для бизнес-правил и CRUD записей табеля.
builder.Services.AddScoped<TimeEntryService>();

var app = builder.Build();

// Создание индексов в MongoDB при запуске приложения.
var indexInitializer = app.Services.GetRequiredService<MongoIndexInitializer>();
await indexInitializer.EnsureIndexesAsync();

// Подключение маршрутизации контроллеров.
app.MapControllers();

// Эндпоинт проверки работоспособности приложения.
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.Run();

static string ToCamelCase(string value)
{
    if (string.IsNullOrEmpty(value))
    {
        return value;
    }

    return char.ToLowerInvariant(value[0]) + value[1..];
}

// Позволяет использовать этот класс в интеграционных тестах.
public partial class Program;
