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
}
