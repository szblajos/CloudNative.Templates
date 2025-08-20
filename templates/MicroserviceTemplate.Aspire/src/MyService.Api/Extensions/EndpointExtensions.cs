using MyService.Api.Endpoints;

namespace MyService.Api.Extensions;

public static class EndpointExtensions
{
    public static void AddEndpoints(this WebApplication app)
    {
        app.MapItemEndpoints();
        // Add other endpoints here as needed
    }
}
