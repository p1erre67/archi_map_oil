using FluentAssertions;
using PriceWatch.Modules.Prices.Domain.Entities;

namespace PriceWatch.UnitTests.SharedKernel;

public class EntityEqualityTests
{
    [Fact]
    public void SameId_ShouldBeEqual()
    {
        var station1 = StationPrice.Create("EXT-001", "A", "addr", "city", "00000", 0, 0);
        var station2 = station1; // meme reference = meme Id

        station1.Should().Be(station2);
    }

    [Fact]
    public void DifferentId_ShouldNotBeEqual()
    {
        var station1 = StationPrice.Create("EXT-001", "A", "addr", "city", "00000", 0, 0);
        var station2 = StationPrice.Create("EXT-002", "A", "addr", "city", "00000", 0, 0);

        station1.Should().NotBe(station2);
    }

    [Fact]
    public void NullComparison_ShouldNotBeEqual()
    {
        var station = StationPrice.Create("EXT-001", "A", "addr", "city", "00000", 0, 0);

        station.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void OperatorEquals_ShouldWork()
    {
        var station1 = StationPrice.Create("EXT-001", "A", "addr", "city", "00000", 0, 0);
        var station2 = StationPrice.Create("EXT-002", "B", "addr", "city", "00000", 0, 0);

        (station1 == station2).Should().BeFalse();
        (station1 != station2).Should().BeTrue();
        (station1 == station1).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameEntity_ShouldBeSame()
    {
        var station = StationPrice.Create("EXT-001", "A", "addr", "city", "00000", 0, 0);

        station.GetHashCode().Should().Be(station.GetHashCode());
    }
}
