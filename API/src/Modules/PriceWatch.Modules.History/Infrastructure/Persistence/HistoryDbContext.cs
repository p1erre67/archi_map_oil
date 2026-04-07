using Microsoft.EntityFrameworkCore;
using PriceWatch.Modules.History.Application.Interfaces;
using PriceWatch.Modules.History.Domain.Entities;
using PriceWatch.Modules.History.Infrastructure.Persistence.Configurations;

namespace PriceWatch.Modules.History.Infrastructure.Persistence;

internal sealed class HistoryDbContext : DbContext, IHistoryUnitOfWork
{
    public HistoryDbContext(DbContextOptions<HistoryDbContext> options) : base(options) { }

    public DbSet<PriceRecord> PriceRecords => Set<PriceRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PriceRecordConfiguration());
    }
}
