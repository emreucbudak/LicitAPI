using Licit.BiddingService.Application.Interfaces;
using Licit.BiddingService.Infrastructure.Grpc;
using Licit.BiddingService.Infrastructure.Services;
using Licit.BiddingService.Infrastructure.Store;
using Licit.WalletService.API.Grpc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Licit.BiddingService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBiddingInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<WalletGrpcOptions>(options =>
            {
                options.Address = configuration[$"{WalletGrpcOptions.SectionName}:Address"]
                    ?? options.Address;
                options.ServiceKey = configuration[$"{WalletGrpcOptions.SectionName}:ServiceKey"];

                if (int.TryParse(
                    configuration[$"{WalletGrpcOptions.SectionName}:DeadlineSeconds"],
                    out var deadlineSeconds))
                {
                    options.DeadlineSeconds = deadlineSeconds;
                }
            });

            services.AddGrpcClient<WalletInternal.WalletInternalClient>((serviceProvider, options) =>
            {
                var walletOptions = serviceProvider.GetRequiredService<IOptions<WalletGrpcOptions>>().Value;
                options.Address = new Uri(walletOptions.Address);
            });

            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var redisConfiguration = configuration["Redis:ConnectionString"]
                    ?? configuration.GetConnectionString("Redis")
                    ?? "localhost:6379";

                return ConnectionMultiplexer.Connect(redisConfiguration);
            });

            services.AddScoped<IWalletClient, WalletGrpcClient>();
            services.AddScoped<IBidStateStore, BidStateStore>();
            services.AddScoped<IBidEmailNotificationPublisher, CapBidEmailNotificationPublisher>();

            return services;
        }
    }
}
