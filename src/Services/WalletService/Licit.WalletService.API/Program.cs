using System.Text;
using FlashMediator;
using FluentValidation;
using Licit.WalletService.API.Middleware;
using Licit.WalletService.Application.Features.CQRS.Wallet.Deposit;
using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// JWT Settings (AuthService ile aynı secret kullanılmalı)
var jwtSecret = builder.Configuration["JwtSettings:Secret"]!;
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]!;
var jwtAudience = builder.Configuration["JwtSettings:Audience"]!;

// Database
builder.Services.AddDbContext<WalletDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// FlashMediator (CQRS)
builder.Services.AddFlashMediator(
    typeof(DepositFundsCommandHandler).Assembly);

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(DepositFundsCommandHandler).Assembly);

// GlobalExceptionHandler
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

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
