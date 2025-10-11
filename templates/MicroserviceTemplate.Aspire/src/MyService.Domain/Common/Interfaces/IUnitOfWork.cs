namespace MyService.Domain.Common.Interfaces;

public interface IUnitOfWork
{
    Task BeginAsync();
    Task CommitAsync();
    Task RollbackAsync();
    Task PublishDomainEventAsync<T>(T @event, string type);
}

