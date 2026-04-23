using FluentAssertions;
using PriceWatch.Modules.Prices.Domain.Entities;
using PriceWatch.Modules.Prices.Domain.Events;

namespace PriceWatch.UnitTests.Domain;

public class StationPriceTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var station = StationPrice.Create(
            "EXT-001", "Station A", "10 rue de Paris",
            "Paris", "75001", 48.856, 2.352, brandId: 42);

        station.ExternalStationId.Should().Be("EXT-001");
        station.StationName.Should().Be("Station A");
        station.Address.Should().Be("10 rue de Paris");
        station.City.Should().Be("Paris");
        station.PostalCode.Should().Be("75001");
        station.Latitude.Should().Be(48.856);
        station.Longitude.Should().Be(2.352);
        station.BrandId.Should().Be(42);
        station.FuelPrices.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldGenerateUniqueId()
    {
        var station1 = StationPrice.Create("EXT-001", "A", "addr", "city", "00000", 0, 0);
        var station2 = StationPrice.Create("EXT-002", "B", "addr", "city", "00000", 0, 0);

        station1.Id.Should().NotBe(station2.Id);
    }

    [Fact]
    public void UpdateInfo_ShouldUpdateProperties()
    {
        var station = StationPrice.Create("EXT-001", "Old", "old addr", "Old City", "00000", 0, 0);
        var beforeUpdate = station.LastUpdated;

        station.UpdateInfo("New", "new addr", "New City", "11111", 1.0, 2.0, brandId: 5);

        station.StationName.Should().Be("New");
        station.Address.Should().Be("new addr");
        station.City.Should().Be("New City");
        station.PostalCode.Should().Be("11111");
        station.Latitude.Should().Be(1.0);
        station.Longitude.Should().Be(2.0);
        station.BrandId.Should().Be(5);
        station.LastUpdated.Should().BeOnOrAfter(beforeUpdate);
    }

    [Fact]
    public void UpsertFuelPrice_NewFuel_ShouldAddToList()
    {
        var station = StationPrice.Create("EXT-001", "A", "addr", "city", "00000", 0, 0);

        station.UpsertFuelPrice("Gazole", 1.85m, DateTime.UtcNow);

        station.FuelPrices.Should().HaveCount(1);
        station.FuelPrices[0].FuelType.Should().Be("Gazole");
        station.FuelPrices[0].PricePerLiter.Should().Be(1.85m);
    }

    [Fact]
    public void UpsertFuelPrice_ExistingFuel_ShouldUpdatePrice()
    {
        var station = StationPrice.Create("EXT-001", "A", "addr", "city", "00000", 0, 0);
        station.UpsertFuelPrice("Gazole", 1.85m, DateTime.UtcNow);

        station.UpsertFuelPrice("Gazole", 1.92m, DateTime.UtcNow);

        station.FuelPrices.Should().HaveCount(1);
        station.FuelPrices[0].PricePerLiter.Should().Be(1.92m);
    }

    [Fact]
    public void UpsertFuelPrice_ShouldUpdateLastUpdated()
    {
        var station = StationPrice.Create("EXT-001", "A", "addr", "city", "00000", 0, 0);
        var before = station.LastUpdated;

        station.UpsertFuelPrice("SP95", 1.95m, DateTime.UtcNow);

        station.LastUpdated.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void MarkAsSynced_ShouldRaiseDomainEvent()
    {
        var station = StationPrice.Create("EXT-001", "A", "addr", "city", "00000", 0, 0);

        station.MarkAsSynced(5);

        station.DomainEvents.Should().HaveCount(1);
        station.DomainEvents[0].Should().BeOfType<StationPricesSyncedEvent>();
    }

    [Fact]
    public void MarkAsSynced_EventShouldContainStationCount()
    {
        var station = StationPrice.Create("EXT-001", "A", "addr", "city", "00000", 0, 0);

        station.MarkAsSynced(42);

        var evt = station.DomainEvents[0] as StationPricesSyncedEvent;
        evt!.StationCount.Should().Be(42);
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmptyList()
    {
        var station = StationPrice.Create("EXT-001", "A", "addr", "city", "00000", 0, 0);
        station.MarkAsSynced(1);
        station.DomainEvents.Should().NotBeEmpty();

        station.ClearDomainEvents();

        station.DomainEvents.Should().BeEmpty();
    }

    // ── Guard Clause Tests ───────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidExternalStationId_ShouldThrow(string? invalidId)
    {
        var act = () => StationPrice.Create(invalidId!, "Station", "addr", "city", "00000", 48.0, 2.0);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidStationName_ShouldThrow(string? invalidName)
    {
        var act = () => StationPrice.Create("EXT-001", invalidName!, "addr", "city", "00000", 48.0, 2.0);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void Create_WithLatitudeOutOfRange_ShouldThrow(double latitude)
    {
        var act = () => StationPrice.Create("EXT-001", "Station", "addr", "city", "00000", latitude, 2.0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public void Create_WithLongitudeOutOfRange_ShouldThrow(double longitude)
    {
        var act = () => StationPrice.Create("EXT-001", "Station", "addr", "city", "00000", 48.0, longitude);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void UpdateInfo_WithInvalidStationName_ShouldThrow(string? invalidName)
    {
        var station = StationPrice.Create("EXT-001", "A", "addr", "city", "00000", 0, 0);

        var act = () => station.UpdateInfo(invalidName!, "addr", "city", "00000", 0, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateInfo_WithLatitudeOutOfRange_ShouldThrow()
    {
        var station = StationPrice.Create("EXT-001", "A", "addr", "city", "00000", 0, 0);

        var act = () => station.UpdateInfo("A", "addr", "city", "00000", 91, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void UpdateInfo_WithLongitudeOutOfRange_ShouldThrow()
    {
        var station = StationPrice.Create("EXT-001", "A", "addr", "city", "00000", 0, 0);

        var act = () => station.UpdateInfo("A", "addr", "city", "00000", 0, 181);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void UpsertFuelPrice_WithInvalidFuelType_ShouldThrow(string? invalidFuelType)
    {
        var station = StationPrice.Create("EXT-001", "A", "addr", "city", "00000", 0, 0);

        var act = () => station.UpsertFuelPrice(invalidFuelType!, 1.85m, DateTime.UtcNow);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpsertFuelPrice_WithInvalidPrice_ShouldThrow(decimal invalidPrice)
    {
        var station = StationPrice.Create("EXT-001", "A", "addr", "city", "00000", 0, 0);

        var act = () => station.UpsertFuelPrice("Gazole", invalidPrice, DateTime.UtcNow);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
