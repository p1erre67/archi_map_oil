using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using PriceWatch.SharedKernel.Domain.Events;

namespace PriceWatch.ArchitectureTests;

/// <summary>
/// Valide les conventions DDD / CQRS :
/// - Les Domain Events sont des records immuables
/// - Les interfaces de repository vivent dans Domain
/// - Les implementations de repository vivent dans Infrastructure
///   et sont sealed (aucune heritage)
/// </summary>
public class DddConventionsTests
{
    private static readonly Assembly PricesAssembly =
        typeof(PriceWatch.Modules.Prices.Infrastructure.DependencyInjection).Assembly;

    private static readonly Assembly HistoryAssembly =
        typeof(PriceWatch.Modules.History.Infrastructure.DependencyInjection).Assembly;

    [Fact]
    public void DomainEvents_ShouldInherit_DomainEventBase()
    {
        var result = Types.InAssembly(PricesAssembly)
            .That().ResideInNamespace("PriceWatch.Modules.Prices.Domain.Events")
            .And().AreClasses()
            .Should().Inherit(typeof(DomainEvent))
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    [Fact]
    public void Prices_RepositoryInterfaces_ShouldResideIn_Domain()
    {
        var result = Types.InAssembly(PricesAssembly)
            .That().AreInterfaces()
            .And().HaveNameEndingWith("Repository")
            .Should().ResideInNamespace("PriceWatch.Modules.Prices.Domain.Repositories")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    [Fact]
    public void History_RepositoryInterfaces_ShouldResideIn_Domain()
    {
        var result = Types.InAssembly(HistoryAssembly)
            .That().AreInterfaces()
            .And().HaveNameEndingWith("Repository")
            .Should().ResideInNamespace("PriceWatch.Modules.History.Domain.Repositories")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    [Fact]
    public void Prices_RepositoryImplementations_ShouldResideIn_Infrastructure()
    {
        var result = Types.InAssembly(PricesAssembly)
            .That().AreClasses()
            .And().HaveNameEndingWith("Repository")
            .And().DoNotHaveNameEndingWith("TestRepository")
            .Should().ResideInNamespace("PriceWatch.Modules.Prices.Infrastructure.Repositories")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    [Fact]
    public void History_RepositoryImplementations_ShouldResideIn_Infrastructure()
    {
        var result = Types.InAssembly(HistoryAssembly)
            .That().AreClasses()
            .And().HaveNameEndingWith("Repository")
            .Should().ResideInNamespace("PriceWatch.Modules.History.Infrastructure.Repositories")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    [Fact]
    public void RepositoryImplementations_ShouldBeSealed()
    {
        var result = Types.InAssembly(PricesAssembly)
            .That().AreClasses()
            .And().ResideInNamespace("PriceWatch.Modules.Prices.Infrastructure.Repositories")
            .Should().BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    [Fact]
    public void CommandAndQueryHandlers_ShouldBeSealed()
    {
        var result = Types.InAssembly(PricesAssembly)
            .That().HaveNameEndingWith("Handler")
            .And().AreClasses()
            .Should().BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    private static string BuildFailureMessage(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "DDD convention violated."
            : "DDD convention violated. Failing types:\n- " +
              string.Join("\n- ", result.FailingTypeNames);
}
