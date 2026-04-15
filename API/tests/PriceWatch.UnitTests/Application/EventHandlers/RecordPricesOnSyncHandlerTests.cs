using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PriceWatch.Modules.History.Application.EventHandlers;
using PriceWatch.Modules.History.Application.Interfaces;
using PriceWatch.Modules.History.Domain.Entities;
using PriceWatch.Modules.History.Domain.Repositories;
using PriceWatch.SharedKernel.Application.Events;

namespace PriceWatch.UnitTests.Application.EventHandlers;

public class RecordPricesOnSyncHandlerTests
{
    private readonly IPriceRecordRepository _repository = Substitute.For<IPriceRecordRepository>();
    private readonly IHistoryUnitOfWork _unitOfWork = Substitute.For<IHistoryUnitOfWork>();
    private readonly FakeLogger _logger = new();
    private readonly RecordPricesOnSyncHandler _handler;

    public RecordPricesOnSyncHandlerTests()
    {
        _handler = new RecordPricesOnSyncHandler(_repository, _unitOfWork, _logger);
    }

    [Fact]
    public async Task Handle_WithStations_ShouldCreateOneRecordPerFuelPrice()
    {
        var notification = CreateEvent(
            new SyncedStationSnapshot("EXT-001", "Station A", "Paris", new List<SyncedFuelPriceSnapshot>
            {
                new("Gazole", 1.85m, DateTime.UtcNow),
                new("SP95", 1.92m, DateTime.UtcNow),
            }),
            new SyncedStationSnapshot("EXT-002", "Station B", "Lyon", new List<SyncedFuelPriceSnapshot>
            {
                new("Gazole", 1.80m, DateTime.UtcNow),
            }));

        List<PriceRecord>? captured = null;
        _repository.When(r => r.AddRange(Arg.Any<IEnumerable<PriceRecord>>()))
            .Do(ci => captured = ci.Arg<IEnumerable<PriceRecord>>().ToList());

        await _handler.Handle(notification, CancellationToken.None);

        _repository.Received(1).AddRange(Arg.Any<IEnumerable<PriceRecord>>());
        captured.Should().NotBeNull();
        captured!.Should().HaveCount(3);
        captured.Should().Contain(r => r.ExternalStationId == "EXT-001" && r.FuelType == "Gazole");
        captured.Should().Contain(r => r.ExternalStationId == "EXT-001" && r.FuelType == "SP95");
        captured.Should().Contain(r => r.ExternalStationId == "EXT-002" && r.FuelType == "Gazole");
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges()
    {
        var notification = CreateEvent(
            new SyncedStationSnapshot("EXT-001", "A", "Paris", new List<SyncedFuelPriceSnapshot>
            {
                new("Gazole", 1.85m, DateTime.UtcNow),
            }));

        await _handler.Handle(notification, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoStations_ShouldCallAddRangeWithEmpty_ThenSave()
    {
        var notification = CreateEvent();

        List<PriceRecord>? captured = null;
        _repository.When(r => r.AddRange(Arg.Any<IEnumerable<PriceRecord>>()))
            .Do(ci => captured = ci.Arg<IEnumerable<PriceRecord>>().ToList());

        await _handler.Handle(notification, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Should().BeEmpty();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnspecifiedKindDates_ShouldBeCoercedToUtc()
    {
        var localDate = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Unspecified);
        var notification = CreateEvent(
            new SyncedStationSnapshot("EXT-001", "A", "Paris", new List<SyncedFuelPriceSnapshot>
            {
                new("Gazole", 1.85m, localDate),
            }));

        List<PriceRecord>? captured = null;
        _repository.When(r => r.AddRange(Arg.Any<IEnumerable<PriceRecord>>()))
            .Do(ci => captured = ci.Arg<IEnumerable<PriceRecord>>().ToList());

        await _handler.Handle(notification, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Should().HaveCount(1);
        captured[0].RecordedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    private static StationPricesSyncedIntegrationEvent CreateEvent(
        params SyncedStationSnapshot[] stations) =>
        new() { Stations = stations };

    /// <summary>
    /// Simple fake logger — NSubstitute can't proxy ILogger of internal types.
    /// </summary>
    private sealed class FakeLogger : ILogger<RecordPricesOnSyncHandler>
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter) { }

        public bool IsEnabled(LogLevel logLevel) => true;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    }
}
