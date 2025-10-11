namespace MyService.Infrastructure.Common.Services;

/// <summary>
/// Represents an outbox service.
/// </summary>
public interface IOutboxService
{
    /// <summary>
    /// Adds a message to the outbox.
    /// </summary>
    /// <param name="message">The message to add.</param>
    /// <param name="type">The type of the message.</param>
    Task AddMessageAsync<T>(T message, string type);
}