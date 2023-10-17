using Grains.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Grains;

public static class GrainsServiceConfigurationExtensions
{
    public static IServiceCollection AddGrains(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddDbContext<MessagingDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("MessagingDb"));
        });



        return services;
    }
}