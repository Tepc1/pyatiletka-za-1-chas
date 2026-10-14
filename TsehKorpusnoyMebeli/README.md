# TsehKorpusnoyMebeli — УП06, вариант 11

Информационная система **Цех корпусной мебели**.

## Состав

| Папка | Описание |
|-------|----------|
| `SQL/` | Скрипты БД `TsehMebeliDB` |
| `API/TsehMebeli/` | ASP.NET Core Web API (.NET 10) |
| `Client/TsehMebeli/` | WinForms-клиент (.NET Framework 4.7.2) |
| `Passwords.txt` | Учебные учётные записи |

## SQL — порядок выполнения

1. `SQL/TsehMebeliDB.sql` — создание БД и таблиц
2. `SQL/TsehMebeliDB_seed.sql` — клиенты, продукты, материалы, операции
3. `SQL/TsehMebeliDB_seed2.sql` — спецификации, пользователи
4. `SQL/TsehMebeliDB_seed3.sql` — заказы, представления, процедуры

## Быстрый старт

1. Выполните SQL-скрипты в SSMS по порядку выше.
2. В `API/TsehMebeli/appsettings.json` укажите свой `Data Source`.
3. Запустите API: `dotnet run --project API/TsehMebeli`.
4. В `Client/TsehMebeli/App.config` проверьте `ApiBaseUrl` (по умолчанию `https://localhost:7150/`).
5. Откройте `Client/TsehMebeli/TsehMebeli.csproj` в Visual Studio и запустите.

## Учётные записи

- `admin` / `Admin123` — полный доступ
- `manager` / `Manager123` — клиенты, заказы, анализ
- `operator` / `Operator123` — только расчёт стоимости заказов
