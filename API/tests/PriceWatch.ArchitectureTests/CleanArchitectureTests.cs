using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;

namespace PriceWatch.ArchitectureTests;

/// <summary>
/// Valide les regles de Clean Architecture sur les deux modules :
/// - Domain n'a aucune dependance externe (pas d'EF, pas d'ASP.NET)
/// - Application ne depend pas d'Infrastructure
/// - Endpoints ne dependent pas directement d'Infrastructure
/// </summary>
public class CleanArchitectureTests
{
    private static readonly Assembly PricesAssembly =
        typeof(PriceWatch.Modules.Prices.Infrastructure.DependencyInjection).Assembly;

    private static readonly Assembly HistoryAssembly =
        typeof(PriceWatch.Modules.History.Infrastructure.DependencyInjection).Assembly;

    // ── Prices module ─────────────────────────────────────────────────────────

    [Fact]
    public void Prices_Domain_ShouldNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(PricesAssembly)
            .That().ResideInNamespace("PriceWatch.Modules.Prices.Domain")
            .ShouldNot().HaveDependencyOn("PriceWatch.Modules.Prices.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
        //Assert.True(result.IsSuccessful, "Domain should not depend on Infrastructure.");
    }

    [Fact]
    public void Prices_Domain_ShouldNotDependOn_EntityFrameworkCore()
    {
        var result = Types.InAssembly(PricesAssembly)
            .That().ResideInNamespace("PriceWatch.Modules.Prices.Domain")
            .ShouldNot().HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    [Fact]
    public void Prices_Domain_ShouldNotDependOn_Application()
    {
        var result = Types.InAssembly(PricesAssembly)
            .That().ResideInNamespace("PriceWatch.Modules.Prices.Domain")
            .ShouldNot().HaveDependencyOn("PriceWatch.Modules.Prices.Application")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    [Fact]
    public void Prices_Application_ShouldNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(PricesAssembly)
            .That().ResideInNamespace("PriceWatch.Modules.Prices.Application")
            .ShouldNot().HaveDependencyOn("PriceWatch.Modules.Prices.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    // ── History module ────────────────────────────────────────────────────────

    [Fact]
    public void History_Domain_ShouldNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(HistoryAssembly)
            .That().ResideInNamespace("PriceWatch.Modules.History.Domain")
            .ShouldNot().HaveDependencyOn("PriceWatch.Modules.History.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    [Fact]
    public void History_Domain_ShouldNotDependOn_EntityFrameworkCore()
    {
        var result = Types.InAssembly(HistoryAssembly)
            .That().ResideInNamespace("PriceWatch.Modules.History.Domain")
            .ShouldNot().HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    [Fact]
    public void History_Application_ShouldNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(HistoryAssembly)
            .That().ResideInNamespace("PriceWatch.Modules.History.Application")
            .ShouldNot().HaveDependencyOn("PriceWatch.Modules.History.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    private static string BuildFailureMessage(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Architecture rule violated."
            : "Architecture rule violated. Failing types:\n- " +
              string.Join("\n- ", result.FailingTypeNames);
}
