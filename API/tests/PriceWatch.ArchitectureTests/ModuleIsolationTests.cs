using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;

namespace PriceWatch.ArchitectureTests;

/// <summary>
/// Valide que les modules du monolithe modulaire restent isoles :
/// aucun module ne doit dependre directement d'un autre module.
/// La communication inter-modules passe exclusivement par les
/// Integration Events du SharedKernel.
/// </summary>
public class ModuleIsolationTests
{
    private static readonly Assembly PricesAssembly =
        typeof(PriceWatch.Modules.Prices.Infrastructure.DependencyInjection).Assembly;

    private static readonly Assembly HistoryAssembly =
        typeof(PriceWatch.Modules.History.Infrastructure.DependencyInjection).Assembly;

    [Fact]
    public void Prices_ShouldNotDependOn_History()
    {
        var result = Types.InAssembly(PricesAssembly)
            .ShouldNot().HaveDependencyOn("PriceWatch.Modules.History")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    [Fact]
    public void History_ShouldNotDependOn_Prices()
    {
        var result = Types.InAssembly(HistoryAssembly)
            .ShouldNot().HaveDependencyOn("PriceWatch.Modules.Prices")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result));
    }

    private static string BuildFailureMessage(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Module isolation rule violated."
            : "Module isolation rule violated. Failing types:\n- " +
              string.Join("\n- ", result.FailingTypeNames);
}
