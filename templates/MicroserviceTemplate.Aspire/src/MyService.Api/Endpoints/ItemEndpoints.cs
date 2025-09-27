using Mediator;
using MyService.Application.Item.Commands;
using MyService.Application.Item.Dtos;
using MyService.Application.Item.Queries;
using MyService.Application.Extensions;
using Microsoft.AspNetCore.Mvc;
using MyService.Domain.Interfaces;
using MyService.Application.Common;

namespace MyService.Api.Endpoints;

public static class ItemEndpoints
{
    public static void MapItemEndpoints(this IEndpointRouteBuilder routes)
    {
        ///
        /// Get all items
        /// This endpoint retrieves all items from the database and caches the response.
        /// 
        routes.MapGet("/items", async (
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize,
            IMediator mediator,
            ICacheService cache) =>
        {

            var pagingParameters = new PagingParameters(pageNumber, pageSize); 

            // Create cache key that includes paging parameters
            string cacheKey = $"items:page:{pagingParameters.PageNumber}:size:{pagingParameters.PageSize}";
            
            // Check cache first
            var cached = await cache.GetResponseAsync<PagedResult<ItemDto>>(cacheKey);
            if (cached is not null)
                return Results.Ok(cached);

            var items = await mediator.Send(new GetItemsQuery(pagingParameters));

            // Cache the response
            if (cached is null && items is not null)
                await cache.SetResponseAsync(cacheKey, items, TimeSpan.FromMinutes(5));

            return Results.Ok(items);
        })
        .WithName("GetItems")
        .WithTags("Items")
        .WithDescription("Retrieves all items from the database, with pagination, and caches the response. Default pageNumber is 1 and default pageSize is 10. If pageSize exceeds 100, it will be set to 100.")
        .WithSummary("Get all items with pagination and caching.")
        .Accepts<int>("pageNumber")
        .Accepts<int>("pageSize")
        .Produces<PagedResult<ItemDto>>(StatusCodes.Status200OK);

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
        .WithDescription("Get an item by ID. This endpoint retrieves a single item by its ID.")
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

                // Remove all cached responses for items (since we have paging now)
                await cache.RemoveByPatternAsync("items:page:*");

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
        .WithDescription("Adds a new item to the database and clears the cache for items.")
        .WithSummary("Creates a new item.")
        .Accepts<CreateItemDto>("application/json")
        .Produces<ItemDto>(StatusCodes.Status201Created)
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);


        routes.MapPut("/items/{id:int}", async (int id, UpdateItemDto dto, IMediator mediator, ICacheService cache) =>
        {
            var command = new UpdateItemCommand { Id = id, Dto = dto };
            try
            {
                await mediator.Send(command);
                await cache.RemoveByPatternAsync("items:page:*");
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
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
        .WithName("UpdateItem")
        .WithTags("Items")
        .WithDescription("Updates the item in the database and clears the cache for items.")
        .WithSummary("Updates an existing item by ID.")
        .Accepts<UpdateItemDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

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

            // Remove all cached responses for items
            await cache.RemoveByPatternAsync("items:page:*");

            return Results.NoContent();
        })
        .WithName("DeleteItem")
        .WithTags("Items")
        .WithDescription("Removes the item from the database and clears the cache.")
        .WithSummary("Deletes an existing item by ID.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}
