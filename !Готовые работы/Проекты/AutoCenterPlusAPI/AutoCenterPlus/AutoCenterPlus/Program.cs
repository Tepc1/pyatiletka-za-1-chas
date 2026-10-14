using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
var configuredConnectionString = builder.Configuration.GetConnectionString("AutoCenterPlus");
if (string.IsNullOrWhiteSpace(configuredConnectionString))
    throw new InvalidOperationException("ConnectionStrings:AutoCenterPlus is not configured.");
var sqlConnection = new SqlConnectionStringBuilder(configuredConnectionString);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".AutoCenterPlus.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization(options => options.FallbackPolicy = options.DefaultPolicy);

var app = builder.Build();
app.Logger.LogInformation(
    "SQL target: {DataSource}; database: {Database}; integrated security: {IntegratedSecurity}; encryption: {Encrypt}; trust server certificate: {TrustServerCertificate}; pooling: {Pooling}; MARS: {MultipleActiveResultSets}",
    sqlConnection.DataSource,
    sqlConnection.InitialCatalog,
    sqlConnection.IntegratedSecurity,
    sqlConnection.Encrypt,
    sqlConnection.TrustServerCertificate,
    sqlConnection.Pooling,
    sqlConnection.MultipleActiveResultSets);
app.Use(async (context, next) =>
{
    var stopwatch = Stopwatch.StartNew();
    try
    {
        await next();
    }
    finally
    {
        stopwatch.Stop();
        app.Logger.LogInformation(
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {ElapsedMilliseconds:0.0} ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.Elapsed.TotalMilliseconds);
    }
});
app.UseExceptionHandler();
app.Use(async (context, next) =>
{
    if (!context.Request.IsHttps)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { error = "HTTPS is required." }, context.RequestAborted);
        return;
    }

    await next();
});
if (app.Environment.IsDevelopment()) app.MapOpenApi().AllowAnonymous();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
