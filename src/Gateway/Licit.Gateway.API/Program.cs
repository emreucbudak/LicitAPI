using System.Linq;
using System.Reflection;
using System.Text;
using Licit.Gateway.API.Dashboard;
using Licit.Gateway.API.Notifications;
using Licit.Gateway.API.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var jwtSecret = builder.Configuration["JwtSettings:Secret"]!;
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]!;
var jwtAudience = builder.Configuration["JwtSettings:Audience"]!;

builder.Services.AddOptions<RedisRateLimitingOptions>()
    .BindConfiguration("RateLimiting")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Redis.ConnectionString),
        "RateLimiting:Redis:ConnectionString must be configured.")
    .Validate(options => options.Policies.Count > 0,
        "At least one rate limiting policy must be configured.")
    .Validate(options => options.Policies.All(policy =>
            !string.IsNullOrWhiteSpace(policy.Name) &&
            !string.IsNullOrWhiteSpace(policy.PathPrefix) &&
            policy.PermitLimit > 0 &&
            policy.WindowSeconds > 0),
        "Each rate limiting policy must define Name, PathPrefix, PermitLimit, and WindowSeconds.")
    .ValidateOnStart();

builder.Services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
{
    var rateLimitOptions = serviceProvider
        .GetRequiredService<IOptions<RedisRateLimitingOptions>>()
        .Value;

    var configuration = ConfigurationOptions.Parse(rateLimitOptions.Redis.ConnectionString!, true);
    configuration.AbortOnConnectFail = false;
    configuration.ClientName = "Licit.Gateway";

    return ConnectionMultiplexer.Connect(configuration);
});
builder.Services.AddSingleton<IRedisRateLimiter, RedisTokenBucketRateLimiter>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<INotificationStore, InMemoryNotificationStore>();
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddSingleton<IUserIdProvider, NotificationUserIdProvider>();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (origins is { Length: > 0 })
        {
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
            return;
        }

        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
            return;
        }

        policy.AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrWhiteSpace(accessToken) &&
                context.HttpContext.Request.Path.StartsWithSegments("/notification-hub"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(NotificationAuth.AccessTokenPolicy, policy =>
        policy.RequireAuthenticatedUser()
            .RequireClaim("tokenType", NotificationAuth.AccessTokenType));
});

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();
app.UseMiddleware<RedisRateLimitingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/gateway", (IHostEnvironment environment) =>
{
    var assembly = Assembly.GetExecutingAssembly().GetName();

    return Results.Ok(new
    {
        Service = "Licit Gateway",
        Environment = environment.EnvironmentName,
        Version = assembly.Version?.ToString()
    });
});

app.MapHealthChecks("/health");
app.MapDashboardSummaryEndpoint();
app.MapNotificationEndpoints();
app.MapHub<NotificationHub>("/notification-hub")
    .RequireAuthorization(NotificationAuth.AccessTokenPolicy);
app.MapReverseProxy();

app.Run();
