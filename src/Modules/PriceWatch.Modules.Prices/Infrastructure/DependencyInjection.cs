using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PriceWatch.Modules.Prices.Application.Interfaces;
using PriceWatch.Modules.Prices.Domain.Repositories;
using PriceWatch.Modules.Prices.Infrastructure.ExternalApi;
using PriceWatch.Modules.Prices.Infrastructure.Persistence;
using PriceWatch.Modules.Prices.Infrastructure.Repositories;

namespace PriceWatch.Modules.Prices.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPricesModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PriceWatch")
            ?? throw new InvalidOperationException("Connection string 'PriceWatch' is not configured.");

        services.AddDbContext<PricesDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString),
                mySql => mySql.MigrationsHistoryTable("__ef_migrations_prices")));

        services.AddScoped<IPricesUnitOfWork>(sp => sp.GetRequiredService<PricesDbContext>());
        services.AddScoped<IStationPriceRepository, StationPriceRepository>();

        var baseUrl = configuration["FuelPriceApi:BaseUrl"]
            ?? "https://api.prix-carburants.2aaz.fr";

        services.AddHttpClient<IFuelPriceApiClient, FuelPriceApiClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        });

        return services;
    }
}
