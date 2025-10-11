using MyService.Domain.Common.Interfaces;
using MyService.Infrastructure.Common.Services;
using MyService.Infrastructure.Data;

namespace MyService.Infrastructure.Common.Data;

/// <summary>
/// Represents a unit of work.
/// </summary>
/// <remarks>
/// This class is responsible for managing database transactions and publishing domain events to the outbox.
/// </remarks>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;
    private readonly IOutboxService _outboxService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="outboxService">The outbox service.</param>
    public UnitOfWork(AppDbContext dbContext, IOutboxService outboxService)
    {
        _dbContext = dbContext;
        _outboxService = outboxService;
    }

    /// <summary>
    /// Begins a new transaction.
    /// </summary>
    /// <returns></returns>
    public async Task BeginAsync() => await _dbContext.Database.BeginTransactionAsync();

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    public async Task CommitAsync()
    {
        await _dbContext.SaveChangesAsync();
        await _dbContext.Database.CommitTransactionAsync();
    }

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    public async Task RollbackAsync() => await _dbContext.Database.RollbackTransactionAsync();

    /// <summary>
    /// Publishes a domain event to the outbox.
    /// </summary>
    public async Task PublishDomainEventAsync<T>(T @event, string type)
    {
        await _outboxService.AddMessageAsync(@event, type);
    }
}
