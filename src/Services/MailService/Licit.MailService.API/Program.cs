using System.Text;
using FlashMediator;
using FluentValidation;
using Licit.MailService.API.Middleware;
using Licit.MailService.Application.DTOs;
using Licit.MailService.Application.Features.CQRS.Email.Send;
using Licit.MailService.Application.Interfaces;
using Licit.MailService.Infrastructure.Data;
using Licit.MailService.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// JWT Settings
var jwtSecret = builder.Configuration["JwtSettings:Secret"]!;
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]!;
var jwtAudience = builder.Configuration["JwtSettings:Audience"]!;

// SMTP Settings
var smtpSettings = builder.Configuration.GetSection("SmtpSettings").Get<SmtpSettings>()!;
builder.Services.AddSingleton(smtpSettings);

// Database
builder.Services.AddDbContext<MailDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddScoped<IEmailSender, MailtrapEmailSender>();

// FlashMediator (CQRS)
builder.Services.AddFlashMediator(typeof(SendEmailCommandHandler).Assembly);

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(SendEmailCommandHandler).Assembly);

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
