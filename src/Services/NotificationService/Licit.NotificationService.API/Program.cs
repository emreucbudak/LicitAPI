using System.Text;
using DotNetCore.CAP;
using Licit.NotificationService.API.Data;
using Licit.NotificationService.API.Notifications;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var jwtSecret = builder.Configuration["JwtSettings:Secret"]!;
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]!;
var jwtAudience = builder.Configuration["JwtSettings:Audience"]!;

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

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<INotificationStore, DbNotificationStore>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddTransient<BiddingOutbidNotificationEventConsumerService>();
builder.Services.AddSingleton<IUserIdProvider, NotificationUserIdProvider>();
builder.Services.AddSignalR();

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
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("NotificationService.JwtBearer");

            logger.LogWarning(
                context.Exception,
                "Notification authentication failed. Path: {Path}",
                context.HttpContext.Request.Path);

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

builder.Services.AddCap(options =>
{
    options.UsePostgreSql(builder.Configuration.GetConnectionString("DefaultConnection")!);
    options.UseRabbitMQ(rabbitMq =>
    {
        var configuredHost = builder.Configuration["RabbitMq:Host"];
        var configuredUsername = builder.Configuration["RabbitMq:Username"];
        var configuredPassword = builder.Configuration["RabbitMq:Password"];
        var configuredExchangeName = builder.Configuration["RabbitMq:ExchangeName"];

        rabbitMq.HostName = string.IsNullOrWhiteSpace(configuredHost) ? "localhost" : configuredHost;
        rabbitMq.Port = builder.Configuration.GetValue<int?>("RabbitMq:Port") ?? 5672;
        rabbitMq.UserName = string.IsNullOrWhiteSpace(configuredUsername) ? "licit" : configuredUsername;
        rabbitMq.Password = string.IsNullOrWhiteSpace(configuredPassword) ? "LicitDev2024!" : configuredPassword;
        rabbitMq.ExchangeName = string.IsNullOrWhiteSpace(configuredExchangeName)
            ? "licit.events"
            : configuredExchangeName;
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Licit Notification Service API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "postgresql");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/notification-service", (IHostEnvironment environment) => Results.Ok(new
{
    Service = "Licit Notification Service",
    Environment = environment.EnvironmentName
}));
app.MapHealthChecks("/health");
app.MapNotificationEndpoints();
app.MapHub<NotificationHub>("/notification-hub")
    .RequireAuthorization(NotificationAuth.AccessTokenPolicy);

app.Run();
