using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using PriceWatch.Modules.Prices.Application.EventHandlers;
using PriceWatch.Modules.Prices.Domain.Entities;
using PriceWatch.Modules.Prices.Domain.Events;
using PriceWatch.Modules.Prices.Domain.Repositories;

namespace PriceWatch.UnitTests.Application.EventHandlers;

public class DetectPriceAnomalyHandlerTests
{
    private readonly IStationPriceRepository _repository = Substitute.For<IStationPriceRepository>();
    private readonly FakeLogger _logger = new();
    private readonly DetectPriceAnomalyHandler _handler;

    public DetectPriceAnomalyHandlerTests()
    {
        _handler = new DetectPriceAnomalyHandler(_repository, _logger);
    }

    [Fact]
    public async Task Handle_NormalPrices_ShouldNotLogWarning()
    {
        var station = CreateStationWithPrice("Gazole", 1.50m);
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<StationPrice> { station });

        await _handler.Handle(new StationPricesSyncedEvent(1), CancellationToken.None);

        _logger.WarningCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_PriceTooLow_ShouldLogWarning()
    {
        var station = CreateStationWithPrice("Gazole", 0.10m);
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<StationPrice> { station });

        await _handler.Handle(new StationPricesSyncedEvent(1), CancellationToken.None);

        _logger.WarningCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_PriceTooHigh_ShouldLogWarning()
    {
        var station = CreateStationWithPrice("SP95", 5.00m);
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<StationPrice> { station });

        await _handler.Handle(new StationPricesSyncedEvent(1), CancellationToken.None);

        _logger.WarningCount.Should().BeGreaterThan(0);
    }

    private static StationPrice CreateStationWithPrice(string fuelType, decimal price)
    {
        var station = StationPrice.Create("EXT-001", "Station A", "addr", "Paris", "75001", 48.856, 2.352);
        station.UpsertFuelPrice(fuelType, price, DateTime.UtcNow);
        return station;
    }

    /// <summary>
    /// Simple fake logger that counts warnings.
    /// NSubstitute can't proxy ILogger of internal types.
    /// </summary>
    private sealed class FakeLogger : ILogger<DetectPriceAnomalyHandler>
    {
        public int WarningCount { get; private set; }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Warning) WarningCount++;
        }

        public bool IsEnabled(LogLevel logLevel) => true;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    }
}
