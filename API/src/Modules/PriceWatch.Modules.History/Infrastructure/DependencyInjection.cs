using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PriceWatch.Modules.History.Application.Interfaces;
using PriceWatch.Modules.History.Domain.Repositories;
using PriceWatch.Modules.History.Infrastructure.Persistence;
using PriceWatch.Modules.History.Infrastructure.Repositories;

namespace PriceWatch.Modules.History.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddHistoryModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PriceWatch")
            ?? throw new InvalidOperationException("Connection string 'PriceWatch' is not configured.");

        services.AddDbContext<HistoryDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history")));

        services.AddScoped<IHistoryUnitOfWork>(sp => sp.GetRequiredService<HistoryDbContext>());
        services.AddScoped<IPriceRecordRepository, PriceRecordRepository>();

        return services;
    }
}
