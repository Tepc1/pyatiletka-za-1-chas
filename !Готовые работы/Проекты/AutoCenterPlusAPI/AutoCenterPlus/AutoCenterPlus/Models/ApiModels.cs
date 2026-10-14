namespace AutoCenterPlus.Models;

public sealed record CustomerDto(int Id, string? Code, string Name, string? Inn, string? Address, string? Phone, bool IsBuyer, bool IsSalesman);
public sealed record CustomerRequest(string Name, string? Inn, string? Address, string? Phone, bool IsBuyer, bool IsSalesman);
public sealed record LoginRequest(string Login, string Password);
public sealed record AuthenticatedUserDto(int Id, string Login, string FullName, string Role);
public sealed record OrderCostDto(int OrderId, decimal TotalCost);
public sealed record OrderSummaryDto(int OrderId, string CustomerName, DateTime OrderDate, decimal TotalCost);
public sealed record OrderLineDto(int OrderId, string CustomerName, DateTime OrderDate, string ProductCode, string ProductName, decimal Quantity, string Unit, decimal UnitCost, decimal TotalPositionCost);
public sealed record ClientOrderAnalysisDto(int ClientId, string ClientName, string ProductName, string ProductCode, decimal Quantity, string Unit, decimal Price, decimal Discount, decimal TotalSum, decimal DiscountPercent, int OrderId);
