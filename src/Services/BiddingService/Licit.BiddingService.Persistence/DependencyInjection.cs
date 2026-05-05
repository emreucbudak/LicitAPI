using DotNetCore.CAP;
using Licit.BiddingService.Application.Interfaces;
using Licit.BiddingService.Application.Repository;
using Licit.BiddingService.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Licit.BiddingService.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBiddingPersistence(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<BiddingDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddCap(options =>
            {
                options.UseEntityFramework<BiddingDbContext>();
                options.UseRabbitMQ(rabbitMqOptions =>
                {
                    rabbitMqOptions.HostName = GetRequiredOrDefault(configuration["RabbitMq:Host"], "localhost");
                    rabbitMqOptions.UserName = GetRequiredOrDefault(configuration["RabbitMq:Username"], "licit");
                    rabbitMqOptions.Password = GetRequiredOrDefault(configuration["RabbitMq:Password"], "LicitDev2024!");
                    rabbitMqOptions.Port = int.TryParse(configuration["RabbitMq:Port"], out var port)
                        ? port
                        : 5672;
                    rabbitMqOptions.ExchangeName = GetRequiredOrDefault(
                        configuration["RabbitMq:ExchangeName"],
                        "licit.events");
                });
            });

            services.AddScoped<IBiddingRepository, BiddingRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }

        private static string GetRequiredOrDefault(string? value, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }
    }
}
