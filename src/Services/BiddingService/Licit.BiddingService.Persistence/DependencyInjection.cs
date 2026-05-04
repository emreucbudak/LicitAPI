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
            services.AddDbContext<BiddingDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IBiddingRepository, BiddingRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
