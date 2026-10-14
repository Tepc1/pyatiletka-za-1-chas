# TsehKorpusnoyMebeli — УП06, вариант 11

Информационная система **Цех корпусной мебели**.

## Visual Studio 2026 — открыть solution

Основной файл solution (формат **.slnx**):

```
TsehKorpusnoyMebeli/TsehMebeli.slnx
```

| Проект | Путь | Назначение |
|--------|------|------------|
| API | `API/TsehMebeli/TsehMebeli.csproj` | ASP.NET Core Web API + Swagger |
| Client | `Client/TsehMebeli/TsehMebeli.csproj` | WinForms (.NET 10) |

> **Важно:** файл `.csproj` — это описание проекта (пакеты, target framework).  
> Файл `.slnx` — это solution (список проектов). Оба нужны: без `.csproj` проект не соберётся.

Можно также открыть solution только API или только Client:
- `API/TsehMebeli/TsehMebeli.slnx`
- `Client/TsehMebeli/TsehMebeli.slnx`

## SQL Server

Строка подключения (API и Client):

```
Data Source=209-U\SQLEXPRESS01;
Initial Catalog=TsehMebeliDB;
User ID=sa;
Password=0000;
Encrypt=True; TrustServerCertificate=True;
```

- API: `API/TsehMebeli/appsettings.json` → `ConnectionStrings:TsehMebeli`
- Client: `Client/TsehMebeli/App.config` → `connectionStrings/TsehMebeli`  
  + класс `Services/SqlConnectionFactory.cs` для прямого доступа к БД

## SQL-скрипты

1. `SQL/TsehMebeliDB.sql`
2. `SQL/TsehMebeliDB_seed.sql`
3. `SQL/TsehMebeliDB_seed2.sql`
4. `SQL/TsehMebeliDB_seed3.sql`

## Запуск

1. Выполнить SQL-скрипты в SSMS
2. Запустить API (F5) → https://localhost:7150/swagger
3. Запустить Client

## Учётки

| Логин | Пароль | Роль |
|-------|--------|------|
| admin | Admin123 | Admin |
| manager | Manager123 | Manager |
| operator | Operator123 | Operator |
