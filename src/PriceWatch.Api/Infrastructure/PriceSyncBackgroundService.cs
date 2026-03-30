using MediatR;
using Microsoft.Extensions.Options;
using PriceWatch.Modules.Prices.Application.Commands.SyncStationPrices;
using PriceWatch.Modules.Prices.Application.Interfaces;

namespace PriceWatch.Api.Infrastructure;

/// <summary>
/// Background service that periodically fetches fuel prices from the external API
/// and syncs them into the database via the SyncStationPricesCommand.
/// </summary>
public sealed class PriceSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly PriceSyncOptions _options;
    private readonly ILogger<PriceSyncBackgroundService> _logger;

    public PriceSyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<PriceSyncOptions> options,
        ILogger<PriceSyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initial delay to allow the application to fully start
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncPricesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Price sync failed. Will retry in {IntervalMinutes} minutes.", _options.IntervalMinutes);
            }

            await Task.Delay(TimeSpan.FromMinutes(_options.IntervalMinutes), stoppingToken);
        }
    }

    private async Task SyncPricesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting price sync for lat={Lat}, lon={Lon}, radius={Radius}km.",
            _options.Latitude, _options.Longitude, _options.RadiusKm);

        await using var scope = _scopeFactory.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var apiClient = scope.ServiceProvider.GetRequiredService<IFuelPriceApiClient>();

        var stations = await apiClient.GetStationsAsync(
            _options.Latitude,
            _options.Longitude,
            _options.RadiusKm,
            cancellationToken);

        var result = await sender.Send(new SyncStationPricesCommand(stations), cancellationToken);

        if (result.IsSuccess)
            _logger.LogInformation("Price sync completed: {Count} station(s) synced.", stations.Count);
        else
            _logger.LogWarning("Price sync returned failure: {Error}", result.Error);
    }
}

public sealed class PriceSyncOptions
{
    public const string SectionName = "PriceSync";

    public int IntervalMinutes { get; set; } = 30;
    public double Latitude { get; set; } = 48.8566;
    public double Longitude { get; set; } = 2.3522;
    public int RadiusKm { get; set; } = 5;
}
