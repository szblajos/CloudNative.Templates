using Mediator;
using MyService.Application.Item.Commands;
using MyService.Application.Item.Dtos;
using MyService.Application.Item.Queries;
using MyService.Application.Extensions;
using Microsoft.AspNetCore.Mvc;
using MyService.Domain.Interfaces;

namespace MyService.Api.Endpoints;

public static class ItemEndpoints
{
    public static void MapItemEndpoints(this IEndpointRouteBuilder routes)
    {
        ///
        /// Get all items
        /// This endpoint retrieves all items from the database and caches the response.
        /// 
        routes.MapGet("/items", async (IMediator mediator,  ICacheService cache) =>
        {
            const string cacheKey = "items:all";
            // Check cache first
            var cached = await cache.GetResponseAsync<IEnumerable<ItemDto>>(cacheKey);
            if (cached is not null)
                return Results.Ok(cached);

            var items = await mediator.Send(new GetItemsQuery());

            // Cache the response
            if (cached is null && items is not null)
                await cache.SetResponseAsync(cacheKey, items, TimeSpan.FromMinutes(5));

            return Results.Ok(items);
        })
        .WithName("GetItems")
        .WithTags("Items")
        .WithDescription("Get all items")
        .WithSummary("Retrieves all items from the database and caches the response.")
        .Accepts<IEnumerable<ItemDto>>("application/json")
        .Produces<IEnumerable<ItemDto>>(StatusCodes.Status200OK);

        ///
        /// Get an item by ID
        /// This endpoint retrieves a single item by its ID.
        ///
        routes.MapGet("/items/{id:int}", async (int id, IMediator mediator) =>
        {
            var item = await mediator.Send(new GetItemsByIdQuery(id));
            return item is not null ? Results.Ok(item) : Results.NotFound();
        })
        .WithName("GetItemById")
        .WithTags("Items")
        .WithDescription("Get an item by ID")
        .WithSummary("Retrieves a single item by its ID.")
        .Produces<ItemDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        ///
        /// Create a new item
        /// This endpoint adds a new item to the database and clears the cache for items.
        /// 
        routes.MapPost("/items", async (CreateItemDto dto, IMediator mediator, ICacheService cache) =>
        {
            try
            {
                var result = await mediator.Send(new CreateItemCommand(dto));

                // Remove cached response for items
                const string cacheKey = "items:all";
                await cache.RemoveResponseAsync(cacheKey);

                return Results.Created($"/items/{result.Id}", result);
            }
            catch (FluentValidation.ValidationException ex)
            {
                return Results.ValidationProblem(ex.Errors.ToDictionary());
            }
            catch (Exception ex)
            {
                // Replace this with your own logging mechanism
                Console.WriteLine(ex);
                // Consider returning a more specific error response
                return Results.Problem("An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("CreateItem")
        .WithTags("Items")
        .WithDescription("Create a new item")
        .WithSummary("Adds a new item to the database and clears the cache for items.")
        .Accepts<CreateItemDto>("application/json")
        .Produces<ItemDto>(StatusCodes.Status201Created)
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);

        ///
        /// Delete an item by ID
        /// This endpoint removes the item from the database and clears the cache.
        ///
        routes.MapDelete("/items/{id:int}", async (int id, IMediator mediator, ICacheService cache) =>
        {
            try
            {
                await mediator.Send(new DeleteItemCommand(id));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }

            // Remove cached response
            const string cacheKey = "items:all";
            await cache.RemoveResponseAsync(cacheKey);

            return Results.NoContent();
        })
        .WithName("DeleteItem")
        .WithTags("Items")
        .WithDescription("Delete an item by ID")
        .WithSummary("Removes the item from the database and clears the cache.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}
