using FluentAssertions;
using NetArchTest.Rules;

namespace PriceWatch.ArchitectureTests.Modules.Cards;

public sealed class CardsArchitectureTests
{
    private static readonly Types CardsTypes =
        Types.InAssembly(typeof(PriceWatch.Modules.Cards.Infrastructure.DependencyInjection).Assembly);

    [Fact]
    public void Domain_ShouldNot_DependOn_Application()
    {
        var result = CardsTypes
            .That().ResideInNamespace("PriceWatch.Modules.Cards.Domain")
            .ShouldNot().HaveDependencyOn("PriceWatch.Modules.Cards.Application")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_ShouldNot_DependOn_Infrastructure()
    {
        var result = CardsTypes
            .That().ResideInNamespace("PriceWatch.Modules.Cards.Domain")
            .ShouldNot().HaveDependencyOn("PriceWatch.Modules.Cards.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_ShouldNot_DependOn_Infrastructure()
    {
        var result = CardsTypes
            .That().ResideInNamespace("PriceWatch.Modules.Cards.Application")
            .ShouldNot().HaveDependencyOn("PriceWatch.Modules.Cards.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void CommandHandlers_ShouldBe_Sealed()
    {
        var result = CardsTypes
            .That().HaveNameEndingWith("CommandHandler")
            .Should().BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void QueryHandlers_ShouldBe_Sealed()
    {
        var result = CardsTypes
            .That().HaveNameEndingWith("QueryHandler")
            .Should().BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Repositories_ShouldReside_InInfrastructure()
    {
        var result = CardsTypes
            .That().HaveNameEndingWith("Repository")
            .And().DoNotHaveNameStartingWith("I")
            .Should().ResideInNamespace("PriceWatch.Modules.Cards.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void RepositoryInterfaces_ShouldReside_InDomain()
    {
        var result = CardsTypes
            .That().HaveNameEndingWith("Repository")
            .And().AreInterfaces()
            .Should().ResideInNamespace("PriceWatch.Modules.Cards.Domain")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void DomainEvents_ShouldReside_InDomain()
    {
        var result = CardsTypes
            .That().HaveNameEndingWith("Event")
            .Should().ResideInNamespace("PriceWatch.Modules.Cards.Domain")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Validators_ShouldReside_InApplication()
    {
        var result = CardsTypes
            .That().HaveNameEndingWith("Validator")
            .Should().ResideInNamespace("PriceWatch.Modules.Cards.Application")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
