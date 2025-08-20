using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MyService.Domain.Entities;

namespace MyService.Domain.Interfaces
{
    public interface IOutboxMessageRepository
    {
        Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int take, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
