using FlashMediator;
using FluentValidation;
using Licit.AuctionService.API.ExceptionHandlers;
using Licit.AuctionService.Application.Feature.CQRS.Auction.Command.CreateAuction;
using Licit.AuctionService.Application.Interface;
using Licit.AuctionService.Application.Repository;
using Licit.AuctionService.Application.Validators;
using Licit.AuctionService.Persistence.Data;
using Licit.AuctionService.Persistence.Repository;
using Microsoft.EntityFrameworkCore;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
builder.Services.AddDbContext<AuctionDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddValidatorsFromAssemblyContaining<AuctionValidator>();
builder.Services.AddFlashMediator(typeof(CreateAuctionCommandHandler).Assembly);
builder.Services.AddSerilog(opt =>
{
    opt
    .WriteTo
    .Console();
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
