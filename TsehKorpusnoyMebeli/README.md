# TsehKorpusnoyMebeli — УП06, вариант 11

Информационная система **Цех корпусной мебели**.

## Visual Studio 2026

Откройте:

```
TsehKorpusnoyMebeli/TsehMebeli.slnx
```

| Проект | Путь |
|--------|------|
| API | `API/TsehMebeli/TsehMebeli.csproj` |
| Client | `Client/TsehMebeli/TsehMebeli.csproj` |

## SQL Server

**Сервер:** `HOME-PC\SQLEXPRESS`  
**БД:** `TsehMebeliDB`  
**Авторизация:** Windows (Integrated Security)

```
Data Source=HOME-PC\SQLEXPRESS;
Initial Catalog=TsehMebeliDB;
Integrated Security=True;
Encrypt=True; TrustServerCertificate=True;
```

- API: `API/TsehMebeli/appsettings.json`
- Client: `Client/TsehMebeli/App.config` + `SqlConnectionFactory`

## SQL-скрипты

1. `SQL/TsehMebeliDB.sql`
2. `SQL/TsehMebeliDB_seed.sql`
3. `SQL/TsehMebeliDB_seed2.sql`
4. `SQL/TsehMebeliDB_seed3.sql`

## Учётки приложения

| Логин | Пароль | Роль |
|-------|--------|------|
| admin | Admin123 | Admin |
| manager | Manager123 | Manager |
| operator | Operator123 | Operator |
