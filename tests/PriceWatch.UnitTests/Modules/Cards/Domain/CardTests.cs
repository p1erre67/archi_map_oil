using FluentAssertions;
using PriceWatch.Modules.Cards.Domain.Entities;
using PriceWatch.Modules.Cards.Domain.Enums;
using PriceWatch.Modules.Cards.Domain.Events;

namespace PriceWatch.UnitTests.Modules.Cards.Domain;

public sealed class CardTests
{
    // ── Card.Create ───────────────────────────────────────────────────────────

    [Fact]
    public void Create_ShouldSucceed_WithValidHolderName()
    {
        var result = Card.Create("Alice Dupont");

        result.IsSuccess.Should().BeTrue();
        result.Value.HolderName.Should().Be("Alice Dupont");
        result.Value.Balance.Should().Be(0m);
        result.Value.Status.Should().Be(CardStatus.Active);
        result.Value.CardNumber.Value.Should().StartWith("PW-").And.HaveLength(11);
    }

    [Fact]
    public void Create_ShouldRaiseCardCreatedEvent()
    {
        var result = Card.Create("Alice Dupont");

        result.Value.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CardCreatedEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldFail_WhenHolderNameIsEmpty(string holderName)
    {
        var result = Card.Create(holderName);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Validation");
    }

    [Fact]
    public void Create_ShouldTrimHolderName()
    {
        var result = Card.Create("  Bob Martin  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.HolderName.Should().Be("Bob Martin");
    }

    // ── Card.TopUp ────────────────────────────────────────────────────────────

    [Fact]
    public void TopUp_ShouldIncreaseBalance_WhenAmountIsPositive()
    {
        var card = Card.Create("Alice").Value;

        var result = card.TopUp(100m);

        result.IsSuccess.Should().BeTrue();
        card.Balance.Should().Be(100m);
    }

    [Fact]
    public void TopUp_ShouldRaiseCardToppedUpEvent()
    {
        var card = Card.Create("Alice").Value;
        card.ClearDomainEvents();

        card.TopUp(50m);

        card.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CardToppedUpEvent>();
    }

    [Fact]
    public void TopUp_ShouldAccumulateBalance_OnMultipleTopUps()
    {
        var card = Card.Create("Alice").Value;

        card.TopUp(100m);
        card.TopUp(50m);

        card.Balance.Should().Be(150m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void TopUp_ShouldFail_WhenAmountIsZeroOrNegative(decimal amount)
    {
        var card = Card.Create("Alice").Value;

        var result = card.TopUp(amount);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("InvalidTopUpAmount");
    }

    [Fact]
    public void TopUp_ShouldFail_WhenCardIsBlocked()
    {
        var card = Card.Create("Alice").Value;
        card.Block();

        var result = card.TopUp(100m);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Blocked");
    }

    // ── Card.RecordFuelTransaction ─────────────────────────────────────────────

    [Fact]
    public void RecordFuelTransaction_ShouldDeductBalance_WhenSufficientFunds()
    {
        var card = Card.Create("Alice").Value;
        card.TopUp(200m);
        card.ClearDomainEvents();

        var result = card.RecordFuelTransaction("SP95", 30m, 1.80m, "STATION-001");

        result.IsSuccess.Should().BeTrue();
        card.Balance.Should().Be(200m - 30m * 1.80m);
    }

    [Fact]
    public void RecordFuelTransaction_ShouldRaiseFuelTransactionRecordedEvent()
    {
        var card = Card.Create("Alice").Value;
        card.TopUp(200m);
        card.ClearDomainEvents();

        card.RecordFuelTransaction("SP95", 30m, 1.80m, "STATION-001");

        card.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<FuelTransactionRecordedEvent>();
    }

    [Fact]
    public void RecordFuelTransaction_ShouldAddTransactionToHistory()
    {
        var card = Card.Create("Alice").Value;
        card.TopUp(200m);

        card.RecordFuelTransaction("SP95", 30m, 1.80m, "STATION-001");

        card.Transactions.Should().Contain(t =>
            t.FuelType == "SP95" &&
            t.LitersFueled == 30m &&
            t.StationId == "STATION-001");
    }

    [Fact]
    public void RecordFuelTransaction_ShouldFail_WhenInsufficientBalance()
    {
        var card = Card.Create("Alice").Value;
        card.TopUp(10m);

        var result = card.RecordFuelTransaction("SP95", 30m, 1.80m, "STATION-001");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("InsufficientBalance");
    }

    [Fact]
    public void RecordFuelTransaction_ShouldFail_WhenCardIsBlocked()
    {
        var card = Card.Create("Alice").Value;
        card.TopUp(200m);
        card.Block();

        var result = card.RecordFuelTransaction("SP95", 30m, 1.80m, "STATION-001");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Blocked");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void RecordFuelTransaction_ShouldFail_WhenLitersIsZeroOrNegative(decimal liters)
    {
        var card = Card.Create("Alice").Value;
        card.TopUp(200m);

        var result = card.RecordFuelTransaction("SP95", liters, 1.80m, "STATION-001");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("InvalidFuelAmount");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1.5)]
    public void RecordFuelTransaction_ShouldFail_WhenPricePerLiterIsZeroOrNegative(decimal pricePerLiter)
    {
        var card = Card.Create("Alice").Value;
        card.TopUp(200m);

        var result = card.RecordFuelTransaction("SP95", 30m, pricePerLiter, "STATION-001");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("InvalidPricePerLiter");
    }

    // ── Card.Block ────────────────────────────────────────────────────────────

    [Fact]
    public void Block_ShouldSucceed_WhenCardIsActive()
    {
        var card = Card.Create("Alice").Value;

        var result = card.Block();

        result.IsSuccess.Should().BeTrue();
        card.Status.Should().Be(CardStatus.Blocked);
    }

    [Fact]
    public void Block_ShouldFail_WhenCardIsAlreadyBlocked()
    {
        var card = Card.Create("Alice").Value;
        card.Block();

        var result = card.Block();

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("AlreadyBlocked");
    }

    // ── CardNumber ────────────────────────────────────────────────────────────

    [Fact]
    public void CardNumber_ShouldHaveCorrectFormat()
    {
        var card = Card.Create("Alice").Value;

        card.CardNumber.Value.Should().MatchRegex(@"^PW-[A-Z0-9]{8}$");
    }

    [Fact]
    public void CardNumber_ShouldBeUnique_AcrossMultipleCards()
    {
        var card1 = Card.Create("Alice").Value;
        var card2 = Card.Create("Bob").Value;

        card1.CardNumber.Value.Should().NotBe(card2.CardNumber.Value);
    }
}
