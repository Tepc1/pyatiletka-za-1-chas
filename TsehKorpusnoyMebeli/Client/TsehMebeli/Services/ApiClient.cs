using System.Configuration;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TsehMebeli.Services;

internal sealed class ApiRequestException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public ApiRequestException(HttpStatusCode statusCode, string message) : base(message)
        => StatusCode = statusCode;
}

internal sealed class AuthenticatedUser
{
    public int Id { get; set; }
    public string Login { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "";
}

internal sealed class ApiCustomer
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = "";
    public string? Inn { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsBuyer { get; set; }
    public bool IsSalesman { get; set; }
}

internal sealed class ApiCustomerRequest
{
    public string Name { get; set; } = "";
    public string? Inn { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsBuyer { get; set; }
    public bool IsSalesman { get; set; }
}

internal sealed class ApiOrderCost
{
    public int OrderId { get; set; }
    public decimal TotalCost { get; set; }
}

internal sealed class ApiOrderSummary
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = "";
    public DateTime OrderDate { get; set; }
    public decimal TotalCost { get; set; }
}

internal sealed class ApiAnalysisRow
{
    public int ClientId { get; set; }
    public string ClientName { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string ProductCode { get; set; } = "";
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "";
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalSum { get; set; }
    public decimal DiscountPercent { get; set; }
    public int OrderId { get; set; }
}

internal static class ApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly HttpClient Client = CreateClient();

    public static async Task<AuthenticatedUser?> LoginAsync(string login, string password)
    {
        using var response = await Client.PostAsJsonAsync("api/auth/login",
            new { login, password }, JsonOptions).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return null;
        return await ReadJsonAsync<AuthenticatedUser>(response).ConfigureAwait(false);
    }

    public static async Task LogoutAsync()
    {
        using var response = await Client.PostAsync("api/auth/logout",
            new StringContent("", Encoding.UTF8, "application/json")).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public static Task<List<ApiCustomer>> GetCustomersAsync() =>
        GetJsonAsync<List<ApiCustomer>>("api/customers");

    public static Task<ApiCustomer> CreateCustomerAsync(ApiCustomerRequest customer) =>
        SendJsonAsync<ApiCustomer>(HttpMethod.Post, "api/customers", customer);

    public static async Task UpdateCustomerAsync(int id, ApiCustomerRequest customer)
    {
        using var response = await Client.PutAsJsonAsync("api/customers/" + id, customer, JsonOptions).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            await ThrowAsync(response).ConfigureAwait(false);
    }

    public static async Task DeleteCustomerAsync(int id)
    {
        using var response = await Client.DeleteAsync("api/customers/" + id).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            await ThrowAsync(response).ConfigureAwait(false);
    }

    public static Task<ApiOrderCost> CalculateOrderCostAsync(int id) =>
        GetJsonAsync<ApiOrderCost>("api/orders/" + id + "/cost");

    public static Task<List<ApiOrderSummary>> GetOrdersAsync() =>
        GetJsonAsync<List<ApiOrderSummary>>("api/orders");

    public static Task<List<ApiAnalysisRow>> GetAnalysisAsync(int? customerId)
    {
        var path = "api/analysis";
        if (customerId.HasValue)
            path += "?customerId=" + customerId.Value;
        return GetJsonAsync<List<ApiAnalysisRow>>(path);
    }

    private static HttpClient CreateClient()
    {
        var configuredAddress = ConfigurationManager.AppSettings["ApiBaseUrl"];
        if (!Uri.TryCreate(configuredAddress, UriKind.Absolute, out var address) ||
            (address.Scheme != Uri.UriSchemeHttps && address.Scheme != Uri.UriSchemeHttp))
            throw new ConfigurationErrorsException("ApiBaseUrl должен содержать абсолютный HTTP(S)-адрес API.");

        var handler = new HttpClientHandler
        {
            CookieContainer = new CookieContainer(),
            UseCookies = true
        };
        return new HttpClient(handler) { BaseAddress = new Uri(address.AbsoluteUri.TrimEnd('/') + "/") };
    }

    private static async Task<T> GetJsonAsync<T>(string path)
    {
        using var response = await Client.GetAsync(path).ConfigureAwait(false);
        return await ReadJsonAsync<T>(response).ConfigureAwait(false);
    }

    private static async Task<T> SendJsonAsync<T>(HttpMethod method, string path, object payload)
    {
        using var request = new HttpRequestMessage(method, path)
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };
        using var response = await Client.SendAsync(request).ConfigureAwait(false);
        return await ReadJsonAsync<T>(response).ConfigureAwait(false);
    }

    private static async Task<T> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
            await ThrowAsync(response).ConfigureAwait(false);
        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions).ConfigureAwait(false);
        return result ?? throw new ApiRequestException(response.StatusCode, "Пустой ответ API.");
    }

    private static async Task ThrowAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var message = response.ReasonPhrase ?? "Ошибка";
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("error", out var err))
                message = err.GetString() ?? message;
            else if (doc.RootElement.TryGetProperty("title", out var title))
                message = title.GetString() ?? message;
        }
        catch (JsonException) { }

        throw new ApiRequestException(response.StatusCode,
            "API вернул HTTP " + (int)response.StatusCode + ": " + message);
    }
}
