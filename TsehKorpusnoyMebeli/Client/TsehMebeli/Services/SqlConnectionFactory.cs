using System.Configuration;
using Microsoft.Data.SqlClient;

namespace TsehMebeli.Services;

/// <summary>
/// Фабрика подключений к TsehMebeliDB (209-U\SQLEXPRESS01, sa).
/// Используется, когда клиенту нужен прямой доступ к SQL (отчёты, диагностика).
/// Основной обмен данными — через API.
/// </summary>
internal static class SqlConnectionFactory
{
    public static string ConnectionString
    {
        get
        {
            var settings = ConfigurationManager.ConnectionStrings["TsehMebeli"];
            if (settings is null || string.IsNullOrWhiteSpace(settings.ConnectionString))
                throw new ConfigurationErrorsException(
                    "В App.config не задана connectionStrings/TsehMebeli.");
            return settings.ConnectionString;
        }
    }

    public static SqlConnection Create()
    {
        return new SqlConnection(ConnectionString);
    }

    /// <summary>Проверка доступности SQL Server.</summary>
    public static async Task<(bool Ok, string Message)> TestAsync(CancellationToken ct = default)
    {
        try
        {
            await using var connection = Create();
            await connection.OpenAsync(ct).ConfigureAwait(false);
            await using var cmd = new SqlCommand("SELECT DB_NAME(), SUSER_SNAME();", connection);
            await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            if (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                var db = reader.IsDBNull(0) ? "?" : reader.GetString(0);
                var login = reader.IsDBNull(1) ? "?" : reader.GetString(1);
                return (true, $"Подключено: БД «{db}», пользователь «{login}».");
            }
            return (true, "Подключение установлено.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
