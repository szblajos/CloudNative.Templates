using MyService.Domain.Items.Entities;
using MyService.Domain.Items.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MyService.Infrastructure.Repositories
{    
    public class ItemsRepository : IItemsRepository
    {
        private readonly Data.AppDbContext _dbContext;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context to be used by the repository.</param>
        public ItemsRepository(Data.AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Gets an item by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the item.</param>
        /// <returns>A task representing the asynchronous operation, containing the item if found, or null otherwise.</returns>
        public async Task<Item?> GetByIdAsync(int id)
        {
            return await _dbContext.Items.FindAsync(id);
        }

        /// <summary>
        /// Gets all items without pagination. Use with caution for large datasets.
        /// Consider using projection methods for better performance.
        /// </summary>
        public async Task<IEnumerable<Item>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Items.ToListAsync(cancellationToken);
        }

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
        public async Task<(IEnumerable<TProjection> Items, int TotalCount)> GetPagedProjectionAsync<TProjection>(
        Func<IQueryable<Item>, IQueryable<TProjection>> projection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Items.AsQueryable();
            var totalCount = await query.CountAsync(cancellationToken);

            var projectedQuery = projection(query);
            var items = await projectedQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        /// <summary>
        /// Gets all projected items using the provided projection function.
        /// This is more efficient than fetching entities and then mapping them.
        /// </summary>
        /// <typeparam name="TProjection">The type to project to.</typeparam>
        /// <param name="projection">A function that defines the projection from IQueryable<Item> to IQueryable<TProjection>.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A list of projected items.</returns>
        public async Task<IEnumerable<TProjection>> GetAllProjectionAsync<TProjection>(
            Func<IQueryable<Item>, IQueryable<TProjection>> projection,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Items.AsQueryable();
            var projectedQuery = projection(query);
            return await projectedQuery.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Adds a new item to the repository.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddAsync(Item item, CancellationToken cancellationToken = default)
        {
            await _dbContext.Items.AddAsync(item, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Updates an existing item in the repository.
        /// </summary>
        /// <param name="item">The item to update.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateAsync(Item item, CancellationToken cancellationToken = default)
        {
            _dbContext.Items.Update(item);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Deletes an item from the repository.
        /// </summary>
        /// <param name="item">The item to delete.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteAsync(Item item, CancellationToken cancellationToken = default)
        {
            if (item != null)
            {
                _dbContext.Items.Remove(item);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
