namespace MyService.Domain.Interfaces;
public interface IMessagePublisher
{
    Task PublishAsync<T>(string type, T message, CancellationToken cancellationToken = default) where T : class;
}