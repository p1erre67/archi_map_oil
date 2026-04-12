using FluentAssertions;
using NSubstitute;
using PriceWatch.Modules.Prices.Application.Commands.SyncStationPrices;
using PriceWatch.Modules.Prices.Application.DTOs;
using PriceWatch.Modules.Prices.Application.Interfaces;
using PriceWatch.Modules.Prices.Domain.Entities;
using PriceWatch.Modules.Prices.Domain.Repositories;

namespace PriceWatch.UnitTests.Application.CommandHandlers;

public class SyncStationPricesCommandHandlerTests
{
    private readonly IStationPriceRepository _stationRepo = Substitute.For<IStationPriceRepository>();
    private readonly IBrandRepository _brandRepo = Substitute.For<IBrandRepository>();
    private readonly IPricesUnitOfWork _unitOfWork = Substitute.For<IPricesUnitOfWork>();
    private readonly SyncStationPricesCommandHandler _handler;

    public SyncStationPricesCommandHandlerTests()
    {
        _handler = new SyncStationPricesCommandHandler(_stationRepo, _brandRepo, _unitOfWork);
    }

    [Fact]
    public async Task Handle_NewStation_ShouldAddToRepository()
    {
        var command = CreateCommand("EXT-001");
        _stationRepo.GetByExternalIdAsync("EXT-001", Arg.Any<CancellationToken>())
            .Returns((StationPrice?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _stationRepo.Received(1).Add(Arg.Any<StationPrice>());
        _stationRepo.DidNotReceive().Update(Arg.Any<StationPrice>());
    }

    [Fact]
    public async Task Handle_ExistingStation_ShouldUpdateInRepository()
    {
        var existing = StationPrice.Create("EXT-001", "Old", "addr", "city", "00000", 0, 0);
        _stationRepo.GetByExternalIdAsync("EXT-001", Arg.Any<CancellationToken>())
            .Returns(existing);

        var command = CreateCommand("EXT-001");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _stationRepo.Received(1).Update(Arg.Is(existing));
        _stationRepo.DidNotReceive().Add(Arg.Any<StationPrice>());
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges()
    {
        _stationRepo.GetByExternalIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((StationPrice?)null);

        var command = CreateCommand("EXT-001");

        await _handler.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldMarkLastStationAsSynced()
    {
        _stationRepo.GetByExternalIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((StationPrice?)null);

        StationPrice? captured = null;
        _stationRepo.When(r => r.Add(Arg.Any<StationPrice>()))
            .Do(ci => captured = ci.Arg<StationPrice>());

        var command = CreateCommand("EXT-001");

        await _handler.Handle(command, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithBrand_ShouldUpsertBrand()
    {
        _stationRepo.GetByExternalIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((StationPrice?)null);
        _brandRepo.GetByExternalIdAsync(42, Arg.Any<CancellationToken>())
            .Returns((Brand?)null);

        var command = CreateCommand("EXT-001", brandId: 42, brandName: "TotalEnergies");

        await _handler.Handle(command, CancellationToken.None);

        _brandRepo.Received(1).Add(Arg.Is<Brand>(b => b.ExternalId == 42));
    }

    [Fact]
    public async Task Handle_WithExistingBrand_ShouldNotAddAgain()
    {
        var existingBrand = Brand.Create(42, "Total", "TOT", 100);
        _stationRepo.GetByExternalIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((StationPrice?)null);
        _brandRepo.GetByExternalIdAsync(42, Arg.Any<CancellationToken>())
            .Returns(existingBrand);

        var command = CreateCommand("EXT-001", brandId: 42, brandName: "TotalEnergies");

        await _handler.Handle(command, CancellationToken.None);

        _brandRepo.DidNotReceive().Add(Arg.Any<Brand>());
    }

    [Fact]
    public async Task Handle_EmptyStations_ShouldReturnSuccess()
    {
        var command = new SyncStationPricesCommand([]);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _stationRepo.DidNotReceive().Add(Arg.Any<StationPrice>());
    }

    private static SyncStationPricesCommand CreateCommand(
        string stationId,
        int? brandId = null,
        string? brandName = null)
    {
        var fuelPrices = new List<ExternalFuelPriceDto>
        {
            new("Gazole", 1.85m, DateTime.UtcNow),
            new("SP95", 1.92m, DateTime.UtcNow)
        };

        var stations = new List<ExternalStationDto>
        {
            new(stationId, "Station A", "10 rue de Paris", "Paris", "75001",
                48.856, 2.352, fuelPrices,
                BrandId: brandId, BrandName: brandName,
                BrandShortName: brandName?[..3], BrandNbStations: 100)
        };

        return new SyncStationPricesCommand(stations);
    }
}
