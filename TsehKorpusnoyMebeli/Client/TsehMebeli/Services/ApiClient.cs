using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TsehMebeli.Services
{
    internal sealed class ApiRequestException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public ApiRequestException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }

    internal sealed class AuthenticatedUser
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
    }

    internal sealed class ApiCustomer
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string INN { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public bool IsBuyer { get; set; }
        public bool IsSalesman { get; set; }
    }

    internal sealed class ApiCustomerRequest
    {
        public string Name { get; set; }
        public string INN { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public bool IsBuyer { get; set; }
        public bool IsSalesman { get; set; }
    }

    internal sealed class ApiOrderCost
    {
        public int OrderId { get; set; }
        public decimal TotalCost { get; set; }
    }

    internal sealed class ApiAnalysisRow
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalSum { get; set; }
        public decimal DiscountPercent { get; set; }
        public int OrderId { get; set; }
    }

    internal static class ApiClient
    {
        private static readonly JavaScriptSerializer serializer = new JavaScriptSerializer();
        private static readonly HttpClient client = CreateClient();

        public static async Task<AuthenticatedUser> LoginAsync(string login, string password)
        {
            using (var content = JsonContent(new { login = login, password = password }))
            using (var response = await client.PostAsync("api/auth/login", content).ConfigureAwait(false))
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return null;
                return await ReadJsonAsync<AuthenticatedUser>(response).ConfigureAwait(false);
            }
        }

        public static async Task LogoutAsync()
        {
            using (var response = await client.PostAsync("api/auth/logout", new StringContent("", Encoding.UTF8, "application/json")).ConfigureAwait(false))
                response.EnsureSuccessStatusCode();
        }

        public static Task<List<ApiCustomer>> GetCustomersAsync()
        {
            return GetJsonAsync<List<ApiCustomer>>("api/customers");
        }

        public static Task<ApiCustomer> CreateCustomerAsync(ApiCustomerRequest customer)
        {
            return SendJsonAsync<ApiCustomer>("POST", "api/customers", customer);
        }

        public static async Task UpdateCustomerAsync(int id, ApiCustomerRequest customer)
        {
            using (var response = await client.PutAsync("api/customers/" + id, JsonContent(customer)).ConfigureAwait(false))
                response.EnsureSuccessStatusCode();
        }

        public static async Task DeleteCustomerAsync(int id)
        {
            using (var response = await client.DeleteAsync("api/customers/" + id).ConfigureAwait(false))
                response.EnsureSuccessStatusCode();
        }

        public static Task<ApiOrderCost> CalculateOrderCostAsync(int id)
        {
            return GetJsonAsync<ApiOrderCost>("api/orders/" + id + "/cost");
        }

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
            Uri address;
            if (!Uri.TryCreate(configuredAddress, UriKind.Absolute, out address) || address.Scheme != Uri.UriSchemeHttps)
                throw new ConfigurationErrorsException("ApiBaseUrl должен содержать абсолютный HTTPS-адрес API.");

            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true
            };
            return new HttpClient(handler) { BaseAddress = new Uri(address.AbsoluteUri.TrimEnd('/') + "/") };
        }

        private static async Task<T> GetJsonAsync<T>(string path)
        {
            using (var response = await client.GetAsync(path).ConfigureAwait(false))
                return await ReadJsonAsync<T>(response).ConfigureAwait(false);
        }

        private static async Task<T> SendJsonAsync<T>(string method, string path, object payload)
        {
            using (var request = new HttpRequestMessage(new HttpMethod(method), path) { Content = JsonContent(payload) })
            using (var response = await client.SendAsync(request).ConfigureAwait(false))
                return await ReadJsonAsync<T>(response).ConfigureAwait(false);
        }

        private static async Task<T> ReadJsonAsync<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var message = response.ReasonPhrase;
                try
                {
                    var problem = serializer.Deserialize<Dictionary<string, object>>(json);
                    object value;
                    if (problem != null &&
                        (problem.TryGetValue("error", out value) || problem.TryGetValue("detail", out value) || problem.TryGetValue("title", out value)))
                        message = Convert.ToString(value);
                }
                catch (ArgumentException)
                {
                }

                throw new ApiRequestException(response.StatusCode,
                    "API вернул HTTP " + (int)response.StatusCode + ": " + message);
            }
            return serializer.Deserialize<T>(json);
        }

        private static StringContent JsonContent(object value)
        {
            return new StringContent(serializer.Serialize(value), Encoding.UTF8, "application/json");
        }
    }
}
