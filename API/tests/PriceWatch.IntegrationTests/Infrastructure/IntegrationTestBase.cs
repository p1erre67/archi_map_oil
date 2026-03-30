using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace PriceWatch.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for all integration tests.
/// Creates a fresh DI scope per test to avoid DbContext lifetime conflicts.
/// </summary>
[Collection("Integration")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly PriceWatchWebApplicationFactory _factory;
    private IServiceScope _scope = null!;

    /// <summary>MediatR sender — use to dispatch commands and queries.</summary>
    protected ISender Sender { get; private set; } = null!;

    protected IntegrationTestBase(PriceWatchWebApplicationFactory factory)
        => _factory = factory;

    public Task InitializeAsync()
    {
        _scope = _factory.Services.CreateScope();
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _scope.Dispose();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates an <see cref="HttpClient"/> that targets the in-memory test server.
    /// </summary>
    protected HttpClient CreateHttpClient() => _factory.CreateClient();
}
