using FluentAssertions;
using PriceWatch.IntegrationTests.Infrastructure;
using PriceWatch.Modules.Cards.Application.Commands.CreateCard;
using PriceWatch.Modules.Cards.Application.Commands.RecordFuelTransaction;
using PriceWatch.Modules.Cards.Application.Commands.TopUpCard;
using PriceWatch.Modules.Cards.Application.Queries.GetCard;
using PriceWatch.Modules.Cards.Application.Queries.GetCardTransactions;

namespace PriceWatch.IntegrationTests.Modules.Cards;

public sealed class RecordFuelTransactionIntegrationTests : IntegrationTestBase
{
    public RecordFuelTransactionIntegrationTests(PriceWatchWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task RecordFuelTransaction_ShouldDeductBalance_WhenSufficientFunds()
    {
        var cardId = await CreateCardWithBalanceAsync("Luc Bernard", 200m);

        var command = new RecordFuelTransactionCommand(cardId, "SP95", 30m, 1.80m, "STATION-001");
        var result = await Sender.Send(command);

        result.IsSuccess.Should().BeTrue();

        var card = await Sender.Send(new GetCardQuery(cardId));
        card.Value.Balance.Should().Be(200m - 30m * 1.80m);
    }

    [Fact]
    public async Task RecordFuelTransaction_ShouldAppearInTransactionHistory()
    {
        var cardId = await CreateCardWithBalanceAsync("Claire Fontaine", 200m);

        await Sender.Send(new RecordFuelTransactionCommand(cardId, "Gazole", 20m, 1.72m, "STATION-042"));

        var transactionsResult = await Sender.Send(new GetCardTransactionsQuery(cardId));

        transactionsResult.IsSuccess.Should().BeTrue();
        transactionsResult.Value.Should().Contain(t =>
            t.FuelType == "Gazole" &&
            t.LitersFueled == 20m &&
            t.StationId == "STATION-042");
    }

    [Fact]
    public async Task RecordFuelTransaction_ShouldFail_WhenInsufficientBalance()
    {
        var cardId = await CreateCardWithBalanceAsync("Henri Petit", 10m);

        var command = new RecordFuelTransactionCommand(cardId, "SP95", 30m, 1.80m, "STATION-001");
        var result = await Sender.Send(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("InsufficientBalance");

        // Balance should remain unchanged
        var card = await Sender.Send(new GetCardQuery(cardId));
        card.Value.Balance.Should().Be(10m);
    }

    [Fact]
    public async Task RecordFuelTransaction_ShouldFail_WhenCardDoesNotExist()
    {
        var command = new RecordFuelTransactionCommand(Guid.NewGuid(), "SP95", 30m, 1.80m, "STATION-001");
        var result = await Sender.Send(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<Guid> CreateCardWithBalanceAsync(string holderName, decimal balance)
    {
        var createResult = await Sender.Send(new CreateCardCommand(holderName));
        createResult.IsSuccess.Should().BeTrue();
        var cardId = createResult.Value;

        var topUpResult = await Sender.Send(new TopUpCardCommand(cardId, balance));
        topUpResult.IsSuccess.Should().BeTrue();

        return cardId;
    }
}
