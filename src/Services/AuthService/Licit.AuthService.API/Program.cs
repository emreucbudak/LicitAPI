using System.Text;
using FlashMediator;
using FluentValidation;
using Licit.AuthService.API.Middleware;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Features.CQRS.Auth.Login;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Licit.AuthService.Infrastructure.Data;
using Licit.AuthService.Infrastructure.Repositories;
using Licit.AuthService.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// JWT Settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
builder.Services.AddSingleton(jwtSettings);

// Database
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Services
builder.Services.AddScoped<ITokenService, TokenService>();

// FlashMediator (CQRS)
builder.Services.AddFlashMediator(
    typeof(LoginCommandHandler).Assembly);

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(LoginCommandHandler).Assembly);

// GlobalExceptionHandler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Authentication
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
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
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
