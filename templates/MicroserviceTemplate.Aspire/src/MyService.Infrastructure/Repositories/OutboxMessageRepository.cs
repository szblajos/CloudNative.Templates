using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyService.Domain.Entities;
using MyService.Domain.Interfaces;
using MyService.Infrastructure.Data;

namespace MyService.Infrastructure.Repositories
{
    public class OutboxMessageRepository : IOutboxMessageRepository
    {
        private readonly AppDbContext _dbContext;
        public OutboxMessageRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int take, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<OutboxMessage>()
                .Where(m => m.ProcessedAt == null)
                .OrderBy(m => m.CreatedAt)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
