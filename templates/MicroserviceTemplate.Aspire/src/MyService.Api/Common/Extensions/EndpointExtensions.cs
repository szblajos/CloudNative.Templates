using MyService.Api.Items.Endpoints;

namespace MyService.Api.Common.Extensions;

public static class EndpointExtensions
{
    public static void AddEndpoints(this WebApplication app)
    {
        app.MapItemsEndpoints();
        // Add other endpoints here as needed
    }
}
