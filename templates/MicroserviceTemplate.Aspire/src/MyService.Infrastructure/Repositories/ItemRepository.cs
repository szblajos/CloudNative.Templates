using MyService.Domain.Entities;
using MyService.Domain.Interfaces;

namespace MyService.Infrastructure.Repositories
{
    using Microsoft.EntityFrameworkCore;
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

        public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Items.CountAsync(cancellationToken);
        }

        public async Task<IEnumerable<Item>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Items
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Item>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Items.ToListAsync(cancellationToken);
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
