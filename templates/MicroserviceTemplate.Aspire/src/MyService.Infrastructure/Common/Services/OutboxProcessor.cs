using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using MyService.Domain.Common.Interfaces;

namespace MyService.Infrastructure.Common.Services;

/// <summary>
/// Represents a background service that processes outbox messages.
/// </summary>
public class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxProcessor"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    public OutboxProcessor(IServiceProvider serviceProvider, ILogger<OutboxProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Executes the background service.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Outbox Processor started.");

        while (!cancellationToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
            var publisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

            var messages = await outboxRepo.GetUnprocessedMessagesAsync(50, cancellationToken);

            foreach (var message in messages)
            {
                try
                {
                    await publisher.PublishAsync(message.Type, message.Content, cancellationToken);
                    message.ProcessedAt = DateTime.UtcNow;
                    _logger.LogInformation($"Published message {message.Id} of type {message.Type}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(exception: ex, message: $"Failed to publish message {message.Id}");
                }
            }

            await outboxRepo.SaveChangesAsync(cancellationToken);
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        }
        _logger.LogInformation("Outbox Processor stopped.");
    }
}
