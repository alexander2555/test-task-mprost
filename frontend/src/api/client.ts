// Тип ответа об ошибке API с кодом, сообщением и опциональными деталями валидации
export type ApiErrorResponse = {
  code: string
  message: string
  errors?: Record<string, string[]>
}

// Класс ошибки API с HTTP-статусом и структурированным ответом об ошибке
export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly response: ApiErrorResponse,
  ) {
    super(response.message)
  }
}

// Универсальная функция для выполнения HTTP-запросов к API с обработкой ошибок и JSON
export async function apiFetch<T>(
  input: RequestInfo | URL,
  init?: RequestInit,
): Promise<T> {
  const response = await fetch(input, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...init?.headers,
    },
  })

  if (!response.ok) {
    let error: ApiErrorResponse = {
      code: 'request_failed',
      message: 'Не удалось выполнить запрос.',
    }

    try {
      error = (await response.json()) as ApiErrorResponse
    } catch {
      // Keep fallback error when the response is not JSON.
    }

    throw new ApiError(response.status, error)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}
