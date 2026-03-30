using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PriceWatch.Modules.Cards.Application.Interfaces;
using PriceWatch.Modules.Cards.Domain.Repositories;
using PriceWatch.Modules.Cards.Infrastructure.Persistence;
using PriceWatch.Modules.Cards.Infrastructure.Repositories;

namespace PriceWatch.Modules.Cards.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCardsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PriceWatch")
            ?? throw new InvalidOperationException("Connection string 'PriceWatch' is not configured.");

        services.AddDbContext<CardsDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString),
                mySql => mySql.MigrationsHistoryTable("__ef_migrations_cards")));

        services.AddScoped<ICardsUnitOfWork>(sp => sp.GetRequiredService<CardsDbContext>());
        services.AddScoped<ICardRepository, CardRepository>();

        return services;
    }
}
