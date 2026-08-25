# NOTES

## Текущие решения и допущения

- Backend: .NET 8, обычные application services без MediatR.
- MongoDB: официальный `MongoDB.Driver`.
- Frontend: React + TypeScript, TanStack Query для server state, React Hook Form для форм.
- Таблицы — обычные React/HTML-компоненты.
- Итоги табеля (`TotalHours`, `TotalAmount`) считаются MongoDB aggregation по всей отфильтрованной выборке; постранично загружаются только строки текущей страницы.
- Календарные даты передаются через API как `YYYY-MM-DD`.
- Историческая стоимость не хранится как неизменяемый snapshot: ставка определяется на дату записи из актуальной истории ставок.
- Optimistic concurrency применяется к редактированию `TimeEntry`: update использует `Id + Version` и увеличивает `Version` после успешного сохранения. DELETE остаётся без version, так как задание отдельно этого не требует.
- Денежные суммы округляются до двух знаков по правилу round-half-to-even: в C# используется `MidpointRounding.ToEven`, в MongoDB — `$round`. Это сохраняет одинаковое поведение list-side и project report, включая midpoint-значения.
- Overtime вычисляется динамически по сумме всех записей сотрудника за календарный день и равен `true` только при сумме строго больше 12 часов.
- Для обычного списка связанные сотрудники и проекты загружаются батчами.
- Project report считается MongoDB aggregation pipeline. История ставок разворачивается, фильтруется по `From <= entry.Date` и сортируется по `From` убыванию, после чего выбирается последняя действующая ставка.
- В project report `risk = percent > 80`, `overspent = percent > 100`.
- Для проекта с нулевым бюджетом процент освоения считается неопределённым (`null`), `risk` и `overspent` — `false`.
- Итоговая строка project report суммирует часы, стоимость и бюджеты проектов с трудозатратами; общий процент считается как `totalAmount / totalBudget * 100`.
- Повторное закрытие уже закрытого периода должно возвращать `409 period_already_closed`; открытие периода, который не был закрыт, должно возвращать `409 period_not_closed`.

## MongoDB: модель хранения и индексы

- Используется официальный `MongoDB.Driver`.
- `Employee.Rates` хранится embedded-массивом: история ставок небольшая, редко меняется и принадлежит сотруднику.
- `TimeEntry` хранит ссылки на сотрудника и проект, часы, дату и `Version`; ставка, стоимость и overtime не сохраняются как source of truth.
- `DateOnly` хранится как BSON `DateTime`, чтобы календарные даты оставались скалярными и могли участвовать в range queries и индексах.
- `decimal` используется для ставок, бюджета и денежных расчётов.
- MongoDB conventions задают camelCase для полей документов.

### Индексы

- `time_entries: { date: 1 }` — первый `$match` project report и выборки за месяц без дополнительных фильтров.
- `time_entries: { employeeId: 1, date: 1 }` — фильтр по сотруднику и проверка суммарных часов сотрудника за день.
- `time_entries: { projectId: 1, date: 1 }` — фильтр списка по проекту и периоду.
- `projects: { code: 1 } unique` — бизнес-требование уникальности шифра проекта.
- `closed_periods: { year: 1, month: 1 } unique` — один закрытый период на календарный месяц и защита от конкурентного повторного закрытия.

## Frontend decisions

- TanStack Query используется для server state: загрузки, кэширования, invalidation после mutations и единообразной обработки состояний запросов. Локальное состояние интерфейса при этом остаётся обычным React state.
- React Hook Form используется для формы записи табеля, чтобы не реализовывать вручную управление значениями, validation state и submit lifecycle.

## Если бы времени было вдвое больше

- Можно добавить integration tests для MongoDB aggregation и основных API acceptance scenarios.
- Уменьшил бы дублирование MongoDB aggregation expressions между time-entry totals и project report, но только после появления достаточного покрытия тестами.
- Улучшил бы UX loading/empty/error states и accessibility форм и таблиц.
