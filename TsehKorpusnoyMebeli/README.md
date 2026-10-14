# TsehKorpusnoyMebeli — УП06, вариант 11

Информационная система **Цех корпусной мебели**.

## Состав

| Папка | Описание |
|-------|----------|
| `SQL/TsehMebeliDB.sql` | Скрипт создания БД `TsehMebeliDB`, справочников, заказов, представлений и процедур |
| `API/TsehMebeli/` | ASP.NET Core Web API (.NET 10) |
| `Client/TsehMebeli/` | WinForms-клиент (.NET Framework 4.7.2) |
| `Passwords.txt` | Учебные учётные записи |

## Быстрый старт

1. Выполните `SQL/TsehMebeliDB.sql` в SQL Server Management Studio.
2. В `API/TsehMebeli/appsettings.json` укажите свой `Data Source`.
3. Запустите API: `dotnet run --project API/TsehMebeli`.
4. В `Client/TsehMebeli/App.config` проверьте `ApiBaseUrl` (по умолчанию `https://localhost:7150/`).
5. Соберите и запустите клиент в Visual Studio.

## Учётные записи

- `admin` / `Admin123` — полный доступ
- `manager` / `Manager123` — клиенты, заказы, анализ
- `operator` / `Operator123` — только расчёт стоимости заказов
