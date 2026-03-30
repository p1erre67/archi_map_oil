using PriceWatch.SharedKernel.Application.Interfaces;
using System.Reflection;

namespace PriceWatch.Api.Extensions;

internal static class WebApplicationExtensions
{
    /// <summary>
    /// Scans the given assemblies for all IEndpoint implementations and maps their routes.
    /// Modules self-register their endpoints — the API host doesn't need to know about them.
    /// </summary>
    internal static WebApplication MapModuleEndpoints(
        this WebApplication app,
        Assembly[] assemblies)
    {
        var endpointTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false });

        foreach (var type in endpointTypes)
        {
            var endpoint = (IEndpoint)Activator.CreateInstance(type)!;
            endpoint.MapEndpoints(app);
        }

        return app;
    }
}
