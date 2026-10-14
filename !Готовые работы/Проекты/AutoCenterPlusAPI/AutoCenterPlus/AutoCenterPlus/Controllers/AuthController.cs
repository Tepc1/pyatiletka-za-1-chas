using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoCenterPlus.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AutoCenterPlus.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IConfiguration configuration, ILogger<AuthController> logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticatedUserDto>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { error = "Логин и пароль обязательны." });

        var connectionString = configuration.GetConnectionString("AutoCenterPlus");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogCritical("ConnectionStrings:AutoCenterPlus is missing.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { error = "API не настроен для подключения к базе AutoCenterPlusDB. Обратитесь к администратору." });
        }

        AuthenticatedUserDto user;
        try
        {
            const string sql = "SELECT Id, PasswordHash, Salt, Role, FullName FROM dbo.[User] WHERE Login = @Login;";
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@Login", SqlDbType.NVarChar, 50).Value = request.Login.Trim();
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return Unauthorized();

            var expectedHash = reader.GetString(1);
            var salt = reader.GetString(2);
            if (!HashesMatch(expectedHash, ComputeHash(request.Password, salt)))
                return Unauthorized();

            user = new AuthenticatedUserDto(
                reader.GetInt32(0),
                request.Login.Trim(),
                reader.IsDBNull(4) ? request.Login.Trim() : reader.GetString(4),
                reader.GetString(3));
        }
        catch (SqlException exception)
        {
            logger.LogError(exception, "Database operation failed during API login.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { error = "API не может прочитать пользователей из AutoCenterPlusDB. Проверьте SQL Server, схему БД и права Windows-учётной записи, под которой запущен API." });
        }

        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role)
        ], CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = false, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

        return Ok(user);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    private static string ComputeHash(string password, string salt)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(password + salt)));
    }

    private static bool HashesMatch(string expected, string actual)
    {
        if (expected.Length != actual.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(expected.ToUpperInvariant()),
            Encoding.ASCII.GetBytes(actual));
    }
}
