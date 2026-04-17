using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseHttpsRedirection();

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
