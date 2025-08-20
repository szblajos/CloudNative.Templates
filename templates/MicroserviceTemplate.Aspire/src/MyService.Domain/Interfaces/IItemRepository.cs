using MyService.Domain.Entities;

namespace MyService.Domain.Interfaces;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(int id);
    Task<IEnumerable<Item>> GetAllAsync();
    Task AddAsync(Item item);
    Task DeleteAsync(Item item);
}
