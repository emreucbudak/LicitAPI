using System.Text;
using FlashMediator;
using FluentValidation;
using Licit.WalletService.API.Grpc;
using Licit.WalletService.API.ExceptionHandlers;
using Licit.WalletService.API.Services;
using Licit.WalletService.Application.Behaviors;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deposit;
using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Application.Services;
using Licit.WalletService.Infrastructure.Data;
using Licit.WalletService.Infrastructure.Repositories;
using Licit.WalletService.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var internalGrpcPort = builder.Configuration.GetValue<int?>("InternalGrpc:Port");
if (internalGrpcPort is > 0)
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(internalGrpcPort.Value, listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http2;
        });
    });
}

// Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// JWT Settings
var jwtSecret = builder.Configuration["JwtSettings:Secret"]!;
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]!;
var jwtAudience = builder.Configuration["JwtSettings:Audience"]!;

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000"];
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Database
builder.Services.AddDbContext<WalletDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IWalletProvisioningService, WalletProvisioningService>();
builder.Services.AddScoped<IDepositIdempotencyStore, RedisDepositIdempotencyStore>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var configuration = builder.Configuration["Redis:ConnectionString"]
        ?? builder.Configuration.GetConnectionString("Redis")
        ?? "localhost:6379";

    return ConnectionMultiplexer.Connect(configuration);
});

// FlashMediator (CQRS)
builder.Services.AddFlashMediator(
    typeof(DepositFundsCommandHandler).Assembly);
builder.Services.AddPipelineBehavior(typeof(LoggingPipelineBehavior<,>));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(DepositFundsCommandHandler).Assembly);

// Exception handlers
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Authentication (AuthService tarafından üretilen JWT'leri doğrular)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
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

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddGrpc();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Licit Wallet Service API", Version = "v1" });
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

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "postgresql");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WalletDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseExceptionHandler();
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
app.MapGrpcService<WalletInternalGrpcService>();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
