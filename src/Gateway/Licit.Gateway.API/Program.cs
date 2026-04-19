using System.Linq;
using System.Reflection;
using Licit.Gateway.API.RateLimiting;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000", "http://localhost:5173"];

        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();
app.UseMiddleware<RedisRateLimitingMiddleware>();

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
app.MapReverseProxy();

app.Run();
