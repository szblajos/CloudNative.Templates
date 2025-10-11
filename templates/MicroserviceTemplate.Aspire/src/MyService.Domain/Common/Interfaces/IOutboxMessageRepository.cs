using MyService.Domain.Common.Entities;

namespace MyService.Domain.Common.Interfaces;

public interface IOutboxMessageRepository
{
    Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int take, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

