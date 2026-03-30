using Microsoft.AspNetCore.Builder;

namespace PriceWatch.SharedKernel.Application.Interfaces;

/// <summary>
/// Contract for registering Minimal API endpoints.
/// Each module implements this to expose its routes without coupling to the API project.
/// The API host discovers all implementations via assembly scanning at startup.
/// </summary>
public interface IEndpoint
{
    void MapEndpoints(WebApplication app);
}
