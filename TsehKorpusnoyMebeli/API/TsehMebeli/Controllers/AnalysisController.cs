using TsehMebeli.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace TsehMebeli.Controllers;

[ApiController]
[Authorize(Roles = "Manager,Admin")]
[Route("api/analysis")]
public sealed class AnalysisController(IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClientOrderAnalysisDto>>> Get([FromQuery] int? customerId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT ClientId, ClientName, ProductName, ProductCode, Quantity, Unit, Price,
                   Discount, TotalSum, DiscountPercent, OrderId
            FROM dbo.v_ClientOrderAnalysis
            WHERE @CustomerId IS NULL OR ClientId = @CustomerId
            ORDER BY ClientName, OrderId, ProductName;
            """;
        var rows = new List<ClientOrderAnalysisDto>();
        await using var connection = new SqlConnection(configuration.GetConnectionString("TsehMebeli"));
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId.HasValue ? customerId.Value : DBNull.Value;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            rows.Add(new ClientOrderAnalysisDto(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetDecimal(4), reader.GetString(5), reader.GetDecimal(6), reader.GetDecimal(7), reader.GetDecimal(8), reader.GetDecimal(9), reader.GetInt32(10)));
        return Ok(rows);
    }
}
