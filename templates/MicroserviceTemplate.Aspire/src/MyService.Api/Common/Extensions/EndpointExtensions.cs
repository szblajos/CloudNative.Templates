using MyService.Api.Item.Endpoints;

namespace MyService.Api.Common.Extensions;

public static class EndpointExtensions
{
    public static void AddEndpoints(this WebApplication app)
    {
        app.MapItemEndpoints();
        // Add other endpoints here as needed
    }
}
