using MyService.Domain.Entities;
using MyService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MyService.Infrastructure.Repositories
{    
    public class ItemRepository : IItemRepository
    {
        private readonly Data.AppDbContext _dbContext;

        public ItemRepository(Data.AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Item?> GetByIdAsync(int id)
        {
            return await _dbContext.Items.FindAsync(id);
        }

        public async Task<IEnumerable<Item>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Items.ToListAsync(cancellationToken);
        }

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

        public async Task<IEnumerable<TProjection>> GetAllProjectionAsync<TProjection>(
            Func<IQueryable<Item>, IQueryable<TProjection>> projection,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Items.AsQueryable();
            var projectedQuery = projection(query);
            return await projectedQuery.ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Item item, CancellationToken cancellationToken = default)
        {
            await _dbContext.Items.AddAsync(item, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Item item, CancellationToken cancellationToken = default)
        {
            _dbContext.Items.Update(item);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

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
