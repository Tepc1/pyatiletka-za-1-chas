# TsehKorpusnoyMebeli — УП06, вариант 11

Информационная система **Цех корпусной мебели**.

## Состав

| Папка | Описание |
|-------|----------|
| `SQL/` | Скрипты БД `TsehMebeliDB` |
| `API/` | ASP.NET Core Web API + Swagger (Visual Studio 2026 / .NET 10) |
| `Client/` | WinForms-клиент (.NET Framework 4.7.2) |
| `Passwords.txt` | Учебные учётные записи |

## SQL — порядок выполнения

1. `SQL/TsehMebeliDB.sql` — создание БД и таблиц
2. `SQL/TsehMebeliDB_seed.sql` — клиенты, продукты, материалы, операции
3. `SQL/TsehMebeliDB_seed2.sql` — спецификации, пользователи
4. `SQL/TsehMebeliDB_seed3.sql` — заказы, представления, процедуры

## API (Visual Studio 2026)

1. Откройте solution: `API/TsehMebeli.sln`
2. В `API/TsehMebeli/appsettings.json` укажите свой `Data Source` (SQL Server)
3. Запустите профиль **https** (F5) — браузер откроет **Swagger UI**:  
   https://localhost:7150/swagger
4. Либо из терминала:
   ```bash
   dotnet restore API/TsehMebeli/TsehMebeli.csproj
   dotnet run --project API/TsehMebeli/TsehMebeli.csproj
   ```

### Endpoints

| Метод | Путь | Роли |
|-------|------|------|
| POST | `/api/auth/login` | anonymous |
| POST | `/api/auth/logout` | authenticated |
| GET/POST/PUT/DELETE | `/api/customers` | Admin, Manager |
| GET | `/api/orders`, `/api/orders/{id}`, `/api/orders/{id}/cost` | Admin, Manager, Operator |
| GET | `/api/analysis` | Admin, Manager |

## Клиент

1. В `Client/TsehMebeli/App.config` проверьте `ApiBaseUrl` (`https://localhost:7150/`)
2. Откройте `Client/TsehMebeli/TsehMebeli.csproj` в Visual Studio и запустите

## Учётные записи

| Логин | Пароль | Роль |
|-------|--------|------|
| admin | Admin123 | Admin |
| manager | Manager123 | Manager |
| operator | Operator123 | Operator |
