using TsehMebeli.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace TsehMebeli.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/customers")]
public sealed class CustomersController(IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerDto>>> Get(CancellationToken cancellationToken)
    {
        var customers = new List<CustomerDto>();
        await using var connection = new SqlConnection(configuration.GetConnectionString("TsehMebeli"));
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("SELECT Id, Code, Name, INN, Address, Phone, IsBuyer, IsSalesman FROM dbo.Customer ORDER BY Name;", connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            customers.Add(new CustomerDto(
                reader.GetInt32(0),
                reader.IsDBNull(1) ? null : reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetString(5),
                reader.GetBoolean(6),
                reader.GetBoolean(7)));
        }
        return Ok(customers);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CustomerRequest request, CancellationToken cancellationToken)
    {
        if (!IsValid(request, out var error))
            return BadRequest(new { error });

        const string sql = """
            INSERT INTO dbo.Customer (Name, INN, Address, Phone, IsBuyer, IsSalesman)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Inn, @Address, @Phone, @IsBuyer, @IsSalesman);
            """;
        await using var connection = new SqlConnection(configuration.GetConnectionString("TsehMebeli"));
        await connection.OpenAsync(cancellationToken);
        await using var command = CreateCustomerCommand(sql, connection, request);
        var id = (int)(await command.ExecuteScalarAsync(cancellationToken)
            ?? throw new InvalidOperationException("Customer insert did not return an ID."));
        var customer = ToDto(id, request);
        return CreatedAtAction(nameof(Get), customer);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CustomerRequest request, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return BadRequest(new { error = "Идентификатор клиента должен быть положительным." });
        if (!IsValid(request, out var error))
            return BadRequest(new { error });

        const string sql = """
            UPDATE dbo.Customer
            SET Name = @Name, INN = @Inn, Address = @Address, Phone = @Phone,
                IsBuyer = @IsBuyer, IsSalesman = @IsSalesman
            WHERE Id = @Id;
            """;
        await using var connection = new SqlConnection(configuration.GetConnectionString("TsehMebeli"));
        await connection.OpenAsync(cancellationToken);
        await using var command = CreateCustomerCommand(sql, connection, request);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        return await command.ExecuteNonQueryAsync(cancellationToken) == 0 ? NotFound() : NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return BadRequest(new { error = "Идентификатор клиента должен быть положительным." });

        await using var connection = new SqlConnection(configuration.GetConnectionString("TsehMebeli"));
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("DELETE FROM dbo.Customer WHERE Id = @Id;", connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        try
        {
            return await command.ExecuteNonQueryAsync(cancellationToken) == 0 ? NotFound() : NoContent();
        }
        catch (SqlException exception) when (exception.Number == 547)
        {
            return Conflict(new { error = "Клиента нельзя удалить: на него ссылаются заказы." });
        }
    }

    private static bool IsValid(CustomerRequest request, out string error)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 200)
        {
            error = "Название клиента обязательно и должно содержать не более 200 символов.";
            return false;
        }
        if (request.Inn?.Trim().Length > 12 || request.Address?.Trim().Length > 500 || request.Phone?.Trim().Length > 30)
        {
            error = "ИНН, адрес или телефон превышает допустимую длину.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static SqlCommand CreateCustomerCommand(string sql, SqlConnection connection, CustomerRequest request)
    {
        var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value = request.Name.Trim();
        command.Parameters.Add("@Inn", SqlDbType.VarChar, 12).Value = Optional(request.Inn);
        command.Parameters.Add("@Address", SqlDbType.NVarChar, 500).Value = Optional(request.Address);
        command.Parameters.Add("@Phone", SqlDbType.NVarChar, 30).Value = Optional(request.Phone);
        command.Parameters.Add("@IsBuyer", SqlDbType.Bit).Value = request.IsBuyer;
        command.Parameters.Add("@IsSalesman", SqlDbType.Bit).Value = request.IsSalesman;
        return command;
    }

    private static object Optional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();

    private static CustomerDto ToDto(int id, CustomerRequest request) =>
        new(id, null, request.Name.Trim(), EmptyToNull(request.Inn), EmptyToNull(request.Address),
            EmptyToNull(request.Phone), request.IsBuyer, request.IsSalesman);

    private static string? EmptyToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
