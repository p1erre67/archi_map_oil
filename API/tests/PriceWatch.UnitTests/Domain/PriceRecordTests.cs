using FluentAssertions;
using PriceWatch.Modules.History.Domain.Entities;

namespace PriceWatch.UnitTests.Domain;

public class PriceRecordTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var recordedAt = DateTime.UtcNow;

        var record = PriceRecord.Create(
            "EXT-001", "Station A", "Paris", "Gazole", 1.85m, recordedAt);

        record.ExternalStationId.Should().Be("EXT-001");
        record.StationName.Should().Be("Station A");
        record.City.Should().Be("Paris");
        record.FuelType.Should().Be("Gazole");
        record.PricePerLiter.Should().Be(1.85m);
        record.RecordedAt.Should().Be(recordedAt);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueId()
    {
        var r1 = PriceRecord.Create("EXT-001", "A", "Paris", "Gazole", 1.85m, DateTime.UtcNow);
        var r2 = PriceRecord.Create("EXT-002", "B", "Lyon", "SP95", 1.90m, DateTime.UtcNow);

        r1.Id.Should().NotBe(r2.Id);
    }
}
