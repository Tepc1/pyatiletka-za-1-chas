# TsehKorpusnoyMebeli — УП06, вариант 11

Информационная система **Цех корпусной мебели**.

## Visual Studio 2026

Откройте единый solution:

```
TsehKorpusnoyMebeli/TsehKorpusnoyMebeli.sln
```

В solution два проекта:

| Проект | Платформа | Описание |
|--------|-----------|----------|
| **TsehMebeli.Api** | .NET 10 | ASP.NET Core Web API + Swagger |
| **TsehMebeli.Client** | .NET 10 Windows | WinForms-клиент |

1. Установите startup project: **TsehMebeli.Api** (или запускайте оба).
2. В `API/TsehMebeli/appsettings.json` — строка к `209-U\SQLEXPRESS01` (sa / 0000).
3. F5 по API → https://localhost:7150/swagger
4. Запустите **TsehMebeli.Client** (правой кнопкой → Debug).

## SQL

По порядку:

1. `SQL/TsehMebeliDB.sql`
2. `SQL/TsehMebeliDB_seed.sql`
3. `SQL/TsehMebeliDB_seed2.sql`
4. `SQL/TsehMebeliDB_seed3.sql`

## Учётные записи

| Логин | Пароль | Роль |
|-------|--------|------|
| admin | Admin123 | Admin |
| manager | Manager123 | Manager |
| operator | Operator123 | Operator |

## Клиент: навигация

- **◀ / ▶** — назад / вперёд по истории экранов
- **⌂** — главное меню
- Боковое меню: Клиенты, Заказы, Расчёт, Анализ
- **Выход** — завершение сессии API
