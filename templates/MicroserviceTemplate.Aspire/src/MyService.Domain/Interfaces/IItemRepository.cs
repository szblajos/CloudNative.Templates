using MyService.Domain.Entities;

namespace MyService.Domain.Interfaces;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(int id);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Item>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<Item>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Item item, CancellationToken cancellationToken = default);
    Task UpdateAsync(Item item, CancellationToken cancellationToken = default);
    Task DeleteAsync(Item item, CancellationToken cancellationToken = default);

}
