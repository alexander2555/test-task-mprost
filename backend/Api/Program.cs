using Api.Infrastructure.Mongo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMongoDb(builder.Configuration);

var app = builder.Build();

var indexInitializer = app.Services.GetRequiredService<MongoIndexInitializer>();
await indexInitializer.EnsureIndexesAsync();

app.MapControllers();
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.Run();

public partial class Program;
