using FlashMediator;
using Licit.BiddingService.API.Hubs;
using Licit.BiddingService.API.Realtime;
using Licit.BiddingService.Application.Features.CQRS.Command.CreateBidCommand;
using Licit.BiddingService.Application.Notifier;
using Licit.BiddingService.Infrastructure;
using Licit.BiddingService.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddScoped<IBiddingNotifier, SignalRBiddingNotifier>();
builder.Services.AddBiddingInfrastructure(builder.Configuration);
builder.Services.AddBiddingPersistence(builder.Configuration);
builder.Services.AddFlashMediator(typeof(CreateBidCommandHandler).Assembly);
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.MapHub<BiddingHub>("/hubs/bidding");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
