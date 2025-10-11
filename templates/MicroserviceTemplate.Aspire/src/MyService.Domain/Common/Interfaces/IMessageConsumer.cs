namespace MyService.Domain.Common.Interfaces;

public interface IMessageConsumer
{
    /// <summary>
    /// Starts the message consumption process.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the message consumption process.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);
}
