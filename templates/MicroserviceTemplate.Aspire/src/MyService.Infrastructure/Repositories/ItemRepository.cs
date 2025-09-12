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

        public async Task<IEnumerable<Item>> GetAllAsync()
        {
            return await _dbContext.Items.ToListAsync();
        }

        public async Task AddAsync(Item item)
        {
            await _dbContext.Items.AddAsync(item);
            await _dbContext.SaveChangesAsync();
        }
        
        public async Task UpdateAsync(Item item)
        {
            _dbContext.Items.Update(item);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Item item)
        {
            if (item != null)
            {
                _dbContext.Items.Remove(item);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
