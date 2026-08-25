# Timesheet

Тестовое задание на позицию Fullstack-разработчика с использованием .NET 8, MongoDB, React и TypeScript.

Реализованы CRUD для записей табеля, бизнес-валидация, фильтрация и пагинация, проектный отчёт, закрытие и открытие периодов, optimistic concurrency и seed-данные для приёмочной проверки.

## Запуск

Требуется Docker с поддержкой Docker Compose.

1. Клонировать репозиторий и перейти в его корневую директорию.

2. Собрать и запустить приложение:

```bash id="vckyxf"
docker compose up --build -d
```

3. Загрузить исходный набор данных для приёмочной проверки:

```bash id="zwfay2"
docker compose exec mongo mongosh timesheet /seed/seed-data.js
```

4. Открыть frontend:

```text id="bbgi89"
http://localhost:3000
```

5. API доступен по адресу:

```text id="z8symf"
http://localhost:5000
```

Seed-команду можно выполнять повторно: она восстанавливает исходный набор данных для приёмочной проверки.

## Тесты

Из корня репозитория:

```bash id="geqht8"
dotnet test
```

Сборка и typecheck frontend:

```bash id="g9hpog"
cd frontend
npm install
npm run build
```

## Документация

- `NOTES.md` — принятые решения, допущения и архитектурные trade-offs.
- `SEED.md` — описание seed-данных и ожидаемых значений для приёмочной проверки.
- `REVIEW.md` — code review файлов из учебного проекта (папка ./code-review).
