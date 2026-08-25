namespace Api.Application.Common;

// Исключение для ошибок API с кодом, сообщением, HTTP-статусом и деталями валидации
public sealed class ApiException : Exception
{
    // Создаёт исключение с кодом ошибки, сообщением, HTTP-статусом и опциональными деталями валидации
    public ApiException(
        string code,
        string message,
        int statusCode,
        IReadOnlyDictionary<string, string[]>? errors = null)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
        Errors = errors;
    }

    // Код ошибки для идентификации типа ошибки клиентом
    public string Code { get; }

    // HTTP-статус код для ответа клиенту
    public int StatusCode { get; }

    // Детали ошибок валидации по полям (для validation_error)
    public IReadOnlyDictionary<string, string[]>? Errors { get; }
}
