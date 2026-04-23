using FluentAssertions;
using PriceWatch.SharedKernel.Domain.Primitives;

namespace PriceWatch.UnitTests.SharedKernel;

public class GuardTests
{
    // ── AgainstNullOrWhiteSpace ──────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AgainstNullOrWhiteSpace_WithInvalidValue_ShouldThrow(string? value)
    {
        var act = () => Guard.AgainstNullOrWhiteSpace(value, "param");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AgainstNullOrWhiteSpace_WithValidValue_ShouldNotThrow()
    {
        var act = () => Guard.AgainstNullOrWhiteSpace("valid", "param");
        act.Should().NotThrow();
    }

    // ── AgainstNegativeOrZero ────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-99.99)]
    public void AgainstNegativeOrZero_WithInvalidValue_ShouldThrow(decimal value)
    {
        var act = () => Guard.AgainstNegativeOrZero(value, "param");
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AgainstNegativeOrZero_WithPositiveValue_ShouldNotThrow()
    {
        var act = () => Guard.AgainstNegativeOrZero(0.01m, "param");
        act.Should().NotThrow();
    }

    // ── AgainstNegative ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void AgainstNegative_WithNegativeValue_ShouldThrow(int value)
    {
        var act = () => Guard.AgainstNegative(value, "param");
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void AgainstNegative_WithZeroOrPositive_ShouldNotThrow(int value)
    {
        var act = () => Guard.AgainstNegative(value, "param");
        act.Should().NotThrow();
    }

    // ── AgainstOutOfRange ────────────────────────────────────────────────────

    [Theory]
    [InlineData(-91, -90, 90)]
    [InlineData(91, -90, 90)]
    [InlineData(-181, -180, 180)]
    [InlineData(181, -180, 180)]
    public void AgainstOutOfRange_WithOutOfRangeValue_ShouldThrow(double value, double min, double max)
    {
        var act = () => Guard.AgainstOutOfRange(value, min, max, "param");
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(-90, -90, 90)]
    [InlineData(90, -90, 90)]
    [InlineData(0, -90, 90)]
    [InlineData(-180, -180, 180)]
    [InlineData(180, -180, 180)]
    public void AgainstOutOfRange_WithValidValue_ShouldNotThrow(double value, double min, double max)
    {
        var act = () => Guard.AgainstOutOfRange(value, min, max, "param");
        act.Should().NotThrow();
    }
}
