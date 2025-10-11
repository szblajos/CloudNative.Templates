using MyService.Domain.Items.Entities;

namespace MyService.Domain.Items.Interfaces;

public interface IItemsRepository
{
    /// <summary>
    /// Gets an item by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the item.</param>
    /// <returns>A task representing the asynchronous operation, containing the item if found, or null otherwise.</returns>
    Task<Item?> GetByIdAsync(int id);

    /// <summary>
    /// Gets all items without pagination. Use with caution for large datasets.
    /// Consider using projection methods for better performance.
    /// </summary>
    Task<IEnumerable<Item>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paged list of projected items using the provided projection function.
    /// This is more efficient than fetching entities and then mapping them.
    /// </summary>
    /// <typeparam name="TProjection">The type to project to.</typeparam>
    /// <param name="projection">A function that defines the projection from IQueryable<Item> to IQueryable<TProjection>.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A tuple containing the projected items and the total count.</returns>
    Task<(IEnumerable<TProjection> Items, int TotalCount)> GetPagedProjectionAsync<TProjection>(
        Func<IQueryable<Item>, IQueryable<TProjection>> projection,
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all projected items using the provided projection function.
    /// This is more efficient than fetching entities and then mapping them.
    /// </summary>
    /// <typeparam name="TProjection">The type to project to.</typeparam>
    /// <param name="projection">A function that defines the projection from IQueryable<Item> to IQueryable<TProjection>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A list of projected items.</returns>
    Task<IEnumerable<TProjection>> GetAllProjectionAsync<TProjection>(
        Func<IQueryable<Item>, IQueryable<TProjection>> projection,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new item to the repository.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(Item item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing item in the repository.
    /// </summary>
    /// <param name="item">The item to update.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(Item item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an item from the repository.
    /// </summary>
    /// <param name="item">The item to delete.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(Item item, CancellationToken cancellationToken = default);

}
