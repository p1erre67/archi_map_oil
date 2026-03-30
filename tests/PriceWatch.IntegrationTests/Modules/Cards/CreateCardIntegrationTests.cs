using FluentAssertions;
using PriceWatch.IntegrationTests.Infrastructure;
using PriceWatch.Modules.Cards.Application.Commands.CreateCard;
using PriceWatch.Modules.Cards.Application.Queries.GetCard;

namespace PriceWatch.IntegrationTests.Modules.Cards;

public sealed class CreateCardIntegrationTests : IntegrationTestBase
{
    public CreateCardIntegrationTests(PriceWatchWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateCard_ShouldPersist_WhenCommandIsValid()
    {
        var command = new CreateCardCommand("Jean Dupont");

        var result = await Sender.Send(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateCard_ShouldBeRetrievable_AfterCreation()
    {
        var command = new CreateCardCommand("Marie Curie");
        var createResult = await Sender.Send(command);
        createResult.IsSuccess.Should().BeTrue();

        var queryResult = await Sender.Send(new GetCardQuery(createResult.Value));

        queryResult.IsSuccess.Should().BeTrue();
        queryResult.Value.HolderName.Should().Be("Marie Curie");
        queryResult.Value.Balance.Should().Be(0m);
        queryResult.Value.Status.Should().Be("Active");
        queryResult.Value.CardNumber.Should().StartWith("PW-");
    }

    [Fact]
    public async Task CreateCard_ShouldFail_WhenHolderNameIsEmpty()
    {
        var command = new CreateCardCommand("");

        var result = await Sender.Send(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Validation");
    }

    [Fact]
    public async Task GetCard_ShouldFail_WhenCardDoesNotExist()
    {
        var queryResult = await Sender.Send(new GetCardQuery(Guid.NewGuid()));

        queryResult.IsFailure.Should().BeTrue();
        queryResult.Error.Code.Should().Contain("NotFound");
    }
}
