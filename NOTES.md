# NOTES

## Текущие решения и допущения

- Backend: .NET 8, обычные application services без MediatR.
- MongoDB: официальный `MongoDB.Driver`.
- Frontend: React + TypeScript, TanStack Query для server state, React Hook Form для форм.
- Таблицы — обычные React/HTML-компоненты.
- Итоги табеля пока считаются по всей выборке, соответствующей фильтрам.
- Календарные даты передаются через API как `YYYY-MM-DD`.
- Историческая стоимость не хранится как неизменяемый snapshot: ставка определяется на дату записи из актуальной истории ставок.
