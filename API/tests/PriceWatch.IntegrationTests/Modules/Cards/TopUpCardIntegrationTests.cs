using FluentAssertions;
using PriceWatch.IntegrationTests.Infrastructure;
using PriceWatch.Modules.Cards.Application.Commands.CreateCard;
using PriceWatch.Modules.Cards.Application.Commands.TopUpCard;
using PriceWatch.Modules.Cards.Application.Queries.GetCard;

namespace PriceWatch.IntegrationTests.Modules.Cards;

public sealed class TopUpCardIntegrationTests : IntegrationTestBase
{
    public TopUpCardIntegrationTests(PriceWatchWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task TopUp_ShouldIncreaseBalance_WhenAmountIsValid()
    {
        var cardId = await CreateCardAsync("Pierre Martin");

        var result = await Sender.Send(new TopUpCardCommand(cardId, 150m));

        result.IsSuccess.Should().BeTrue();

        var card = await Sender.Send(new GetCardQuery(cardId));
        card.Value.Balance.Should().Be(150m);
    }

    [Fact]
    public async Task TopUp_ShouldAccumulate_OnMultipleTopUps()
    {
        var cardId = await CreateCardAsync("Sophie Lemaire");

        await Sender.Send(new TopUpCardCommand(cardId, 100m));
        await Sender.Send(new TopUpCardCommand(cardId, 50m));

        var card = await Sender.Send(new GetCardQuery(cardId));
        card.Value.Balance.Should().Be(150m);
    }

    [Fact]
    public async Task TopUp_ShouldFail_WhenAmountIsZero()
    {
        var cardId = await CreateCardAsync("André Rousseau");

        var result = await Sender.Send(new TopUpCardCommand(cardId, 0m));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TopUp_ShouldFail_WhenCardDoesNotExist()
    {
        var result = await Sender.Send(new TopUpCardCommand(Guid.NewGuid(), 100m));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<Guid> CreateCardAsync(string holderName)
    {
        var result = await Sender.Send(new CreateCardCommand(holderName));
        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }
}
