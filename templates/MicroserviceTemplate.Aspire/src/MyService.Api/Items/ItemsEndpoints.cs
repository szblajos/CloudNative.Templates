using Mediator;
using MyService.Application.Items.Commands;
using MyService.Application.Items.Dtos;
using MyService.Application.Items.Queries;
using MyService.Application.Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using MyService.Domain.Common.Interfaces;
using MyService.Application.Common;

namespace MyService.Api.Items.Endpoints;

public static class ItemsEndpoints
{
    public static void MapItemsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/items").WithTags("Items");

        group.MapGet("/", GetAllItems)
            .WithName("GetItems")
            .WithDescription("Retrieves all items from the database, with pagination, and caches the response. Default pageNumber is 1 and default pageSize is 10. If pageSize exceeds 100, it will be set to 100.")
            .WithSummary("Get all items with pagination and caching.")
            .Accepts<int>("pageNumber")
            .Accepts<int>("pageSize")
            .Produces<PagedResult<ItemDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", GetItemById)
            .WithName("GetItemById")
            .WithDescription("Get an item by ID. This endpoint retrieves a single item by its ID.")
            .WithSummary("Retrieves a single item by its ID.")
            .Produces<ItemDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateItem)
            .WithName("CreateItem")
            .WithDescription("Adds a new item to the database and clears the cache for items.")
            .WithSummary("Creates a new item.")
            .Accepts<CreateItemDto>("application/json")
            .Produces<ItemDto>(StatusCodes.Status201Created)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", UpdateItem)
            .WithName("UpdateItem")
            .WithDescription("Updates the item in the database and clears the cache for items.")
            .WithSummary("Updates an existing item by ID.")
            .Accepts<UpdateItemDto>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", DeleteItem)
            .WithName("DeleteItem")
            .WithDescription("Removes the item from the database and clears the cache.")
            .WithSummary("Deletes an existing item by ID.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    ///
    /// Get all items
    /// This endpoint retrieves all items from the database and caches the response.
    /// 
    private static async Task<IResult> GetAllItems(
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        IMediator mediator,
        ICacheService cache)
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
    }

    ///
    /// Get an item by ID
    /// This endpoint retrieves a single item by its ID.
    ///
    private static async Task<IResult> GetItemById(int id, IMediator mediator)
    {
        var item = await mediator.Send(new GetItemsByIdQuery(id));
        return item is not null ? Results.Ok(item) : Results.NotFound();
    }

    ///
    /// Create a new item
    /// This endpoint adds a new item to the database and clears the cache for items.
    /// 
    private static async Task<IResult> CreateItem(CreateItemDto dto, IMediator mediator, ICacheService cache)
    {
        try
        {
            var result = await mediator.Send(new CreateItemCommand(dto));

            // Remove all cached responses for items (since we have paging now)
            await cache.RemoveByPatternAsync("items:page:*");

            return Results.Created($"/api/items/{result.Id}", result);
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
    }

    ///
    /// Update an existing item
    /// This endpoint updates an item in the database and clears the cache for items.
    ///
    private static async Task<IResult> UpdateItem(int id, UpdateItemDto dto, IMediator mediator, ICacheService cache)
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
    }

    ///
    /// Delete an item by ID
    /// This endpoint removes the item from the database and clears the cache.
    ///
    private static async Task<IResult> DeleteItem(int id, IMediator mediator, ICacheService cache)
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
    }
}
