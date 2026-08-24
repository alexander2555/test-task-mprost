---

---

---

Т Е С Т О В О Е З А Д А Н И Е

## **Учёттрудозатрати**

# **стоимостьработпопроектам**

Fullstack-разработчик: .NET, MongoDB, React + TypeScript

---

---

---

---

---

ОБЪЁМ 6–8 часов

СРОК 5 календарных дней

ЧАСТЕЙ 2 + разбор

ФОРМАТ СДАЧИ ссылка на репозиторий

---

---

## **Опроекте**

ERP-система для проектной организации: договоры и проекты, сотрудники, табель рабочего времени, зарплата и премии, накладные расходы, рентабельность, склад, документооборот. Система работает в продакшене

несколько лет, база живая, пользователи реальные.

---

Backend

.NET5(C#),REST+SignalR,CQRSчерезMediatR,FluentValidation

База

MongoDB — единственное хранилище, реляционной БД нет: агрегации, change

streams Frontend

React16+TypeScript,mobx-state-tree,Formik+Yup,ag-Grid,Blueprint.js

Прочее

Docker,Hangfire,генерациядокументовизшаблонов

Работа примерно на 80 % — доработка и починка существующего кода, на

20 % — новые модули. Поэтому задание состоит из двух частей: прочитать

чужой код и написать свой.

---

## **Правила**

Ориентировочное время — 6–8 часов. Не нужно вылизывать: нужно, чтобы работало и было понятно, почему сделано именно так.

---

---

---

1 / 12


---

---

---

---

**Вопросы приветствуются.** Спецификация намеренно оставляет несколько мест недосказанными. Уточняющий вопрос — это плюс к оценке, а не минус. Решилинеспрашивать—зафиксируйтедопущениев NOTES.md.

**ИИ-ассистенты разрешены** — Claude Code, Copilot, Cursor: мы сами ими пользуемся. Условие одно: на разборе вы объясняете любую строку и

говорите, где вы с ассистентом не согласились.

**Дизайн не оценивается.** Оцениваются корректность, структура и

обработкаошибок.

---

Ч А С Т Ь 1 · 1 – 1 , 5 Ч А С А

## **Codereview**

Ниже два файла из учебного проекта, написанные «как получилось». Они компилируются и в happy path работают. Задача —** найти проблемы и**

**отранжировать их по важности**.

Оформитеответвфайле REVIEW.md:

Список найденных проблем : файл и фрагмент , в чём суть , **чем** **это** **грозит** **в** **продакшене** , как чинить . Сортировка — от самой опасной к косметике .

Отдельно : что бы вы изменили в *структуре* этого кода , а не в отдельных

строках.

Одну - две проблемы , которые считаете самыми важными , исправьте прямо в

коде — положите исправленные файлы рядом, например TimesheetReportHandler.fixed.cs .

---

---

TimesheetReportHandler.cs — отчёт «стоимость трудозатрат по проектам за месяц»

---

// Учебный проект. Обработчик отчёта "стоимость трудозатрат по проектам за месяц".

// Код рабочий: на небольшой базе отчёт строится и цифры выглядят правдоподобно.

using System; using System.Collections.Generic; using System.Linq; using System.Threading; using System.Threading.Tasks; using MediatR; using MongoDB.Driver;

namespace Demo.Api.Queries.Reports { public class ProjectReportRow { public string ProjectId { get; set; } public string ProjectName { get; set; }

---

---

---

---

2 / 12


---

---

---

---

---

public double Hours { get; set; } public double Amount { get; set; } public double Budget { get; set; } public double Percent { get; set; } public bool Overspent { get; set; } }

public class GetProjectReportQuery : IRequest<List<ProjectReportRow>>

{ public int Year { get; set; } public int Month { get; set; } }

public class TimesheetReportHandler : IRequestHandler<GetProjectReportQuery, List<ProjectReportRow>> { private readonly IMongoDatabase _db;

public TimesheetReportHandler(IMongoDatabase db) { _db = db; }

public async Task<List<ProjectReportRow>> Handle(GetProjectReportQuery request, CancellationToken token) {

var entries = await _db.GetCollection<TimeEntry>("time_entries") .Find(FilterDefinition<TimeEntry>.Empty) .ToListAsync();

var monthEntries = entries .Where(e => e.Date.Year == request.Year && e.Date.Month == request.Month) .ToList();

var rows = new Dictionary<string, ProjectReportRow>();

foreach (var entry in monthEntries) { var employee = _db.GetCollection<Employee>("employees") .Find(e => e.Id == entry.EmployeeId) .FirstOrDefaultAsync().Result;

var rate = employee.Rates.FirstOrDefault().Value;

var amount = Math.Round(entry.Hours * rate, 2);

if (!rows.ContainsKey(entry.ProjectId)) { var project = await _db.GetCollection<Project>("projects") .Find(p => p.Id == entry.ProjectId) .FirstOrDefaultAsync();

rows[entry.ProjectId] = new ProjectReportRow { ProjectId = project.Id, ProjectName = project.Name, Budget = project.Budget }; }

3 / 12


---

---

---

---

---

rows[entry.ProjectId].Hours += entry.Hours; rows[entry.ProjectId].Amount += amount; }

foreach (var row in rows.Values) { row.Percent = Math.Round(row.Amount / row.Budget * 100, 2); row.Overspent = row.Percent > 100; }

return rows.Values.OrderBy(r => r.ProjectName).ToList(); } }

// --- сущности (упрощённо) ---

public class TimeEntry { public string Id { get; set; } public string EmployeeId { get; set; } public string ProjectId { get; set; } public DateTime Date { get; set; } public double Hours { get; set; } public string Comment { get; set; } }

public class Employee { public string Id { get; set; } public string Name { get; set; } public List<Rate> Rates { get; set; } }

public class Rate { public DateTime From { get; set; } public double Value { get; set; } }

public class Project { public string Id { get; set; } public string Name { get; set; } public double Budget { get; set; } } }

---

---

---

---

TimeEntriesPage.tsx — экран «Табель»

---

// Учебный проект. Экран "Табель": список записей за месяц с фильтрами и добавлением.

// Код рабочий: записи отображаются, добавление работает.

import React, { useState, useEffect } from "react";

interface Props {

4 / 12


---

---

---

---

---

year: number; month: number; }

export const TimeEntriesPage = (props: Props) => { const [entries, setEntries] = useState<any[]>([]); const [employees, setEmployees] = useState<any[]>([]); const [employeeId, setEmployeeId] = useState(""); const [hours, setHours] = useState(""); const [date, setDate] = useState(""); const [projectId, setProjectId] = useState(""); const [loading, setLoading] = useState(false);

useEffect(() => { load(); });

useEffect(() => { fetch("/api/employees") .then((r) => r.json()) .then((data) => setEmployees(data)); }, []);

const load = async () => { setLoading(true); const response = await fetch("/api/time-entries?year=" + props.year + "&month=" + props.month); const data = await response.json(); setEntries(data); setLoading(false); };

const filtered = employeeId ? entries.filter((e) => e.employeeId == employeeId) : entries; let total = 0; for (let i = 0; i < filtered.length; i++) { total = total + parseFloat(filtered[i].amount); }

const save = async () => { const body = { employeeId: employeeId, projectId: projectId, date: new Date(date).toLocaleDateString(), hours: hours, };

await fetch("/api/time-entries", { method: "PUT", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body), });

entries.push(body); setEntries(entries); alert("Сохранено"); };

const remove = async (id: string) => {

5 / 12


---

---

---

---

---

await fetch("/api/time-entries/" + id, { method: "DELETE" }); load(); };

return ( <div style={{ padding: 20 }}> <h2>Табель за {props.month}.{props.year}</h2> <select value={employeeId} onChange={(e) => setEmployeeId(e.target.value)}>

<option value="">Все сотрудники</option> {employees.map((emp, index) => ( <option key={index} value={emp.id}> {emp.name} </option> ))} </select>

<div style={{ marginTop: 20 }}> <input placeholder="Дата" value={date} onChange={(e) => setDate(e.target.value)}

/> <input placeholder="Проект" value={projectId} onChange={(e) => setProjectId(e.target.value)} /> <input placeholder="Часы" value={hours} onChange={(e) => setHours(e.target.value)} /> <button onClick={save}>Добавить</button> </div>

{loading && <div>Загрузка...</div>}

<table style={{ marginTop: 20, width: "100%" }}> <tbody> {filtered.map((entry, index) => ( <tr key={index}> <td>{entry.date}</td> <td>{entry.employeeName}</td> <td>{entry.projectName}</td> <td>{entry.hours}</td> <td>{entry.amount.toFixed(2)}</td> <td> <button onClick={() => remove(entry.id)}>Удалить</button> </td> </tr> ))} </tbody> </table> <div style={{ marginTop: 10 }}>Итого: {total.toFixed(2)} руб.</div>

</div> ); };

---

---

---

Ч А С Т Ь 2 · 4 – 6 Ч А С О В

---

---

---

---

6 / 12


---

---

---

---

**Мини-фича end-to-end**

Отдельныймаленькийпроектнатомжестеке—**учёттрудозатрати**

**стоимость работ по проектам**.

**Домен**

| СУЩНОСТЬ | ПОЛЯ |
| --- | --- |
| Employee ФИО,отдел;историячасовыхставок—списокпар«ставка,действуетссотрудник даты».Ставкадействуетсуказаннойдатыдоначаласледующей.Ставкизадаются заранее, меняются редко, но задним числом их менять можно. |
| Project шифр(уникальный,напримерП-001),название,бюджетврублях,датапроект началаидатаокончания.Окончаниеможетотсутствовать—проектбессрочный. |
| TimeEntry сотрудник,проект,дата,количествочасов,комментарий;служебныеполяназапись вашеусмотрение—ктоикогдасоздалилиизменил,версиязаписи.табеля |
| ClosedPeriod годимесяц,закрытыедляредактирования:ихзакрываетбухгалтерия. закрытыйпериод |
| Бизнес-правила 1.Стоимостьзаписи записи—нетекущая ставки, запись создать | = часы × ставка сотрудника, действовавшая на дату.Еслинадатузаписиусотрудникаещёнетниоднойнельзя. |

Суммарно у сотрудника **за** **один** **календарный** **день** **по** **всем** **проектам** **не** **больше** **24** **часов** . Попытка выйти за лимит — ошибка с внятным текстом .

Если за день у сотрудника получилось **больше** **12** **часов** , запись сохраняется , но день помечается как переработка , и флаг виден в интерфейсе .

В **закрытом** **периоде** записи нельзя создавать , изменять и удалять .

Дата записи должна попадать в **период** **проекта** : не раньше даты начала и

не позже даты окончания, если она задана.

Часы — положительные , **кратные** **0,5** , не больше 24 за одну запись .

7.Деньги— decimal,округлениедокопеек. double и float дляденегне

использовать.

---

---

---

7 / 12


---

---

---

---

8.** Конкурентное редактирование:** если запись изменили после того, как еёоткрыли на редактирование, сохранение завершается понятной ошибкой, а

не молча затирает чужие изменения.

**API**

---

---

---

**МЕТОД**

ПУТЬ

**НАЗНАЧЕНИЕ**

---

---

---

GET

/api/time-entries

постраничныйсписокзамесяцсфильтрами year,

month, employeeId, projectId, page, pageSize;в ответе — ФИО, шифр проекта, часы, применённая ставка, стоимость

---

PUT

/api/time-entries

создатьзапись

---

POST

/api/time- entries/{id}

изменитьзапись

DELETE

/api/time- entries/{id}

удалитьзапись

GET

/api/reports/projects

отчёт по проектам за месяц: year , month

GET

/api/employees

справочникидлявыпадающихсписков

/api/projects

---

POST

/api/periods/close

закрытьиоткрытьмесяц

/api/periods/open

---

**Отчёт по проектам за месяц** возвращает по каждому проекту, где были трудозатраты: часы, стоимость, бюджет, процент освоения бюджета, признак перерасхода (больше 100 %) и признак риска (больше 80 %), плюс

итоговуюстроку.

ТРЕБОВАНИЯ К РЕАЛИЗАЦИИ

Отчёт считается** агрегацией на стороне MongoDB**. Выгружать все записи в память и складывать в C# нельзя: считайте, что записей несколько

миллионов.

Список записей — с реальной пагинацией на стороне БД.

Индексы под используемые запросы созданы явно — скриптом, миграцией

илинастарте—икраткообоснованыв NOTES.md.

---

---

---

8 / 12


---

| 409 | смашиночитаемым |
| --- | --- |
| ,ане | 500 и |

Ошибкибизнес-правил—это 400 или человекочитаемымтекстомнарусском

кодоминеголыйтекст

исключения.

**Интерфейс**

ЭКРАН 1 — ТАБЕЛЬ

ЭКРАН 2 — ОТЧЁТ ПО ПРОЕКТАМ

фильтры: месяц, сотрудник, проект;

выбормесяца;

таблица: дата, сотрудник, проект, часы, ставка, стоимость, комментарий, отметка

таблица: проект, часы, стоимость, бюджет, процент освоения, подсветка перерасхода;

переработки; добавление, редактирование и удаление записи — форма в модальном окне;

итоговаястрока.

ошибкивалидацииссерверавидит пользователь, а не только консоль;

итоги по отфильтрованному списку: часы истоимость.

Управление состоянием — любое: mobx, mobx-state-tree, redux, zustand, react-query.В NOTES.md напишите,почемувыбралиименноэто.

Компонентная библиотека — любая или никакой.

**Техническиетребования**

Backendна.NET;можно.NET 8 —унас.NET 5,нопереносимостьподхода важнее версии. Контроллеры тонкие, бизнес-логика вынесена (MediatR или обычные сервисы — на ваш выбор), валидация входных данных отделена от

бизнес-правил.

MongoDB официальным драйвером. ORM поверх Mongo не использовать.

**Тесты:** минимум пять юнит-тестов на бизнес-правила — выбор ставки по дате, лимит часов за день, закрытый период, границы периода проекта,

округлениеденег.

Запуск: docker compose up либоREADMEнеболеечемизпятишагов. Обязательно — команда или скрипт, наполняющий базу тестовыми

данными.

---

---

---

9 / 12


---

---

---

---

---

---

ЧЕГО ДЕЛАТЬ НЕ НУЖНО

## **Приёмочные**

Наполните убедитесь

СОТРУДНИКИ

| Аутентификации тёмной CI/CD, Kubernetes, | темы | ,анимаций | ,ролей | иправ .Импорта микросервисов | доступа ,экспорта . | . | Красивой | , | печатных | вёрстки | , форм | адаптивности ,уведомлений | , | . |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
|  | , | базу что | этими результаты | проверки данными совпадают | — | этои .Проверять | есть | требуемые | мы | будем | сид | -данные именно | — их | . | и |
| ФИО Иванов Петрова | И | . А | И. .С. | ОТДЕЛ Проектный Проектный | СТАВКИ 500 700 | ₽/ ₽/ | ч ч | с01.01.2026; с01.02.2026 | 600₽ | /ч | с | 01.03.2026 |
| ШИФРП-001П-002 | НАЗВАНИЕ Реконструкция Инженерные | цеха сети | БЮДЖЕТ | 20 5 | 000 000 | ₽ ₽ | ПЕРИОД 01.01.2026 с | 01.03.2026, | – | 31.03.2026 без | даты | окончания |
| ДАТА 20.02.2026 05.03.2026 | ТАБЕЛЯ | СОТРУДНИК Иванов Иванов | ПРОЕКТ П-001 П-001 | ЧАСЫ | 8 8 | ОЖИДАЕМАЯ | 4 | 800 | СТОИМОСТЬ₽—ставка | 4 уже | 000 | ₽ 600 |

ПРОЕКТЫ

ЗАПИСИ

05.03.2026

Петрова

П-001

4

2800₽

06.03.2026

Петрова

П-002

10

7000₽

---

---

---

10 / 12


---

---

---

---

ОТЧЁТ ЗА МАРТ 2026

| ПРОЕКТ | ЧАСЫ | СТОИМОСТЬ | БЮДЖЕТ | ОСВОЕНО |
| --- | --- | --- | --- | --- |
| П-001 12 7600₽ 20000₽ 38% |
| П-002 10 7000₽ 5000₽ 140%—перерасход |
| Итого | 22 | 14600₽ |  |  |
| Отчётзафевраль | 2026:П-001 — 8 | часов, 4 000 ₽, освоено | 20 %. |  |

СЦЕНАРИИ С ОШИБКАМИ

Каждый должен давать понятное сообщение в интерфейсе, а не «500» и не

молчание.

Запись Петровой на 15.01.2026 — на эту дату у неё ещё нет ставки .

Запись Иванову 06.03.2026, 20 часов на П -001 — сохраняется , помечается как

переработка.

Следом ещё одна запись Иванову 06.03.2026, 6 часов — суммарно 26 часов за

день, отказ.

Запись на П -002 датой 20.02.2026 — раньше начала проекта , отказ .

Закрыть февраль 2026 и попробовать изменить запись от 20.02.2026 — отказ .

Часы 0 или 3,7 — ошибка валидации прямо в форме .

Открыть одну и ту же запись в двух вкладках , сохранить в первой , затем во

второй — вторая получает внятный отказ.

Изменить ставку Иванова с 01.03.2026 на 650 ₽ и перестроить отчёт за март :

стоимость записи от 05.03.2026 становится 5 200 ₽.

---

## **Чтоприслать**

Ссылкунарепозиторий— GitHubилиGitLab,публичныйилидоступпо

приглашению. В нём:

---

README.md

какзапуститьзапятьшаговикакзагрузитьтестовыеданные

REVIEW.md

ответпочасти1

---

---

---

11 / 12


---

---

---

---

---

NOTES.md

принятые допущения, объяснение выбранных решений, обоснование индексов, что осознанно не доделано и что сделали бы иначе, будь времени вдвое больше

|  | код тесты | и | история | коммитов | — | не | « | одним | коммитом | » |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Дальше паре | вопросов | —созвон | «а | начас как | :пройдёмся бывы | это | масштабировали | по | вашему | коду, ». | повашему | ревью | ипо |

---

Вопросы по заданию — на почту, с которой вы его получили. Спрашивать можно на любом этапе.

---

---

12 / 12
