using FluentAssertions;
using PriceWatch.Modules.Prices.Domain.Entities;

namespace PriceWatch.UnitTests.Domain;

public class BrandTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var brand = Brand.Create(1, "TotalEnergies", "Total", 3500);

        brand.ExternalId.Should().Be(1);
        brand.Name.Should().Be("TotalEnergies");
        brand.ShortName.Should().Be("Total");
        brand.NbStations.Should().Be(3500);
    }

    [Fact]
    public void UpdateInfo_ShouldChangeValues()
    {
        var brand = Brand.Create(1, "Old", "O", 100);

        brand.UpdateInfo("New", "N", 200);

        brand.Name.Should().Be("New");
        brand.ShortName.Should().Be("N");
        brand.NbStations.Should().Be(200);
    }

    // ── Guard Clause Tests ───────────────────────────────────────────────────

    [Theory]// lance 1 test par inline 
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrow(string? invalidName)
    {
        var act = () => Brand.Create(1, invalidName!, "Short", 100);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidShortName_ShouldThrow(string? invalidShortName)
    {
        var act = () => Brand.Create(1, "Name", invalidShortName!, 100);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNegativeNbStations_ShouldThrow()
    {
        var act = () => Brand.Create(1, "Name", "Short", -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void UpdateInfo_WithInvalidName_ShouldThrow(string? invalidName)
    {
        var brand = Brand.Create(1, "Valid", "V", 100);

        var act = () => brand.UpdateInfo(invalidName!, "V", 100);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void UpdateInfo_WithInvalidShortName_ShouldThrow(string? invalidShortName)
    {
        var brand = Brand.Create(1, "Valid", "V", 100);

        var act = () => brand.UpdateInfo("Valid", invalidShortName!, 100);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateInfo_WithNegativeNbStations_ShouldThrow()
    {
        var brand = Brand.Create(1, "Valid", "V", 100);

        var act = () => brand.UpdateInfo("Valid", "V", -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
