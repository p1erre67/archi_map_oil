using FluentAssertions;
using PriceWatch.Modules.Prices.Domain.Entities;

namespace PriceWatch.UnitTests.Domain;

public class FuelPriceTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var now = DateTime.UtcNow;

        var fuel = FuelPrice.Create("Gazole", 1.85m, now);

        fuel.FuelType.Should().Be("Gazole");
        fuel.PricePerLiter.Should().Be(1.85m);
        fuel.UpdatedAt.Should().Be(now);
    }

    [Fact]
    public void UpdatePrice_ShouldChangeValues()
    {
        var fuel = FuelPrice.Create("SP95", 1.90m, DateTime.UtcNow);
        var newDate = DateTime.UtcNow.AddHours(1);

        fuel.UpdatePrice(1.95m, newDate);

        fuel.PricePerLiter.Should().Be(1.95m);
        fuel.UpdatedAt.Should().Be(newDate);
    }

    // ── Guard Clause Tests ───────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidFuelType_ShouldThrow(string? invalidFuelType)
    {
        var act = () => FuelPrice.Create(invalidFuelType!, 1.85m, DateTime.UtcNow);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidPricePerLiter_ShouldThrow(decimal invalidPrice)
    {
        var act = () => FuelPrice.Create("Gazole", invalidPrice, DateTime.UtcNow);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void UpdatePrice_WithInvalidPrice_ShouldThrow(decimal invalidPrice)
    {
        var fuel = FuelPrice.Create("Gazole", 1.85m, DateTime.UtcNow);

        var act = () => fuel.UpdatePrice(invalidPrice, DateTime.UtcNow);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
