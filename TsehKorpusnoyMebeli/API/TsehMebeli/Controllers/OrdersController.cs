using TsehMebeli.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace TsehMebeli.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager,Operator")]
[Route("api/orders")]
public sealed class OrdersController(IConfiguration configuration) : ControllerBase
{
    [HttpGet("{id:int}/cost")]
    public async Task<ActionResult<OrderCostDto>> CalculateCost(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return BadRequest(new { error = "Номер заказа должен быть положительным." });

        await using var connection = new SqlConnection(configuration.GetConnectionString("TsehMebeli"));
        await connection.OpenAsync(cancellationToken);
        await using (var existsCommand = new SqlCommand("SELECT 1 FROM dbo.[Order] WHERE Id = @OrderId;", connection))
        {
            existsCommand.Parameters.Add("@OrderId", SqlDbType.Int).Value = id;
            if (await existsCommand.ExecuteScalarAsync(cancellationToken) is null)
                return NotFound(new { error = "Заказ не найден" });
        }

        await using var command = new SqlCommand("dbo.sp_CalculateOrderCost", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add("@OrderId", SqlDbType.Int).Value = id;
        var totalCost = command.Parameters.Add("@TotalCost", SqlDbType.Decimal);
        totalCost.Precision = 14;
        totalCost.Scale = 2;
        totalCost.Direction = ParameterDirection.Output;
        await command.ExecuteNonQueryAsync(cancellationToken);
        return Ok(new OrderCostDto(id, Convert.ToDecimal(totalCost.Value, System.Globalization.CultureInfo.InvariantCulture)));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryDto>>> Get(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT o.Id, c.Name, o.OrderDate, COALESCE(SUM(v.TotalPositionCost), 0) AS TotalCost
            FROM dbo.[Order] AS o
            INNER JOIN dbo.Customer AS c ON c.Id = o.CustomerId
            LEFT JOIN dbo.v_OrderFullCost AS v ON v.OrderId = o.Id
            GROUP BY o.Id, c.Name, o.OrderDate
            ORDER BY o.OrderDate DESC, o.Id DESC;
            """;
        var orders = new List<OrderSummaryDto>();
        await using var connection = new SqlConnection(configuration.GetConnectionString("TsehMebeli"));
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            orders.Add(new OrderSummaryDto(reader.GetInt32(0), reader.GetString(1), reader.GetDateTime(2), reader.GetDecimal(3)));
        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<IReadOnlyList<OrderLineDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT OrderId, CustomerName, OrderDate, ProductCode, ProductName, Quantity, Unit,
                   MaterialsCostPerUnit + OperationsCostPerUnit AS UnitCost, TotalPositionCost
            FROM dbo.v_OrderFullCost
            WHERE OrderId = @OrderId
            ORDER BY ProductName;
            """;
        var lines = new List<OrderLineDto>();
        await using var connection = new SqlConnection(configuration.GetConnectionString("TsehMebeli"));
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@OrderId", SqlDbType.Int).Value = id;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            lines.Add(new OrderLineDto(reader.GetInt32(0), reader.GetString(1), reader.GetDateTime(2), reader.GetString(3), reader.GetString(4), reader.GetDecimal(5), reader.GetString(6), reader.GetDecimal(7), reader.GetDecimal(8)));
        if (lines.Count == 0) return NotFound(new { error = "Заказ не найден" });
        return Ok(lines);
    }
}
