using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var configuredConnectionString = builder.Configuration.GetConnectionString("TsehMebeli");
if (string.IsNullOrWhiteSpace(configuredConnectionString))
    throw new InvalidOperationException("ConnectionStrings:TsehMebeli is not configured.");
var sqlConnection = new SqlConnectionStringBuilder(configuredConnectionString);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Цех корпусной мебели API",
        Version = "v1",
        Description = "Web API информационной системы «Цех корпусной мебели» (УП06, вариант 11).\n" +
                      "Аутентификация: cookie-сессия после POST /api/auth/login.",
        Contact = new OpenApiContact { Name = "TsehMebeli" }
    });

    options.AddSecurityDefinition("CookieAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Name = ".TsehMebeli.Session",
        Description = "Cookie-сессия после успешного входа (POST /api/auth/login)."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "CookieAuth" }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".TsehMebeli.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
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
    "SQL target: {DataSource}; database: {Database}; integrated security: {IntegratedSecurity}",
    sqlConnection.DataSource,
    sqlConnection.InitialCatalog,
    sqlConnection.IntegratedSecurity);

app.Use(async (context, next) =>
{
    var stopwatch = Stopwatch.StartNew();
    try { await next(); }
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TsehMebeli API v1");
        options.DocumentTitle = "Цех корпусной мебели — Swagger";
        options.RoutePrefix = "swagger";
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
