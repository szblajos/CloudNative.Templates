namespace MyService.Domain.Interfaces;

public interface IUnitOfWork
{
    Task BeginAsync();
    Task CommitAsync();
    Task RollbackAsync();
    Task PublishDomainEventAsync<T>(T @event, string type);
}

