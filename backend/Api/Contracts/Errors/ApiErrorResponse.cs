namespace Api.Contracts.Errors;

// Стандартный ответ API для ошибок.
public sealed class ApiErrorResponse
{
    public required string Code { get; init; }

    public required string Message { get; init; }

    // Детали ошибок валидации по полям (для validation_error)
    public IReadOnlyDictionary<string, string[]>? Errors { get; init; }
}
