using Licit.BiddingService.API.Hubs;
using Licit.BiddingService.API.Realtime;
using Licit.BiddingService.Application.Notifier;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddScoped<IBiddingNotifier, SignalRBiddingNotifier>();
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
