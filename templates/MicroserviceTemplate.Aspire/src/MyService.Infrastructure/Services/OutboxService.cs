using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MyService.Domain.Entities;
using MyService.Infrastructure.Data;

namespace MyService.Infrastructure.Services;

/// <summary>
/// Represents an outbox service.
/// </summary>
/// <param name="dbContext">The database context.</param>
public class OutboxService(AppDbContext dbContext) : IOutboxService
{
    private readonly AppDbContext _dbContext = dbContext;

    /// <summary>
    /// Adds a message to the outbox.
    /// </summary>
    /// <param name="message">The message to add.</param>
    /// <param name="type">The type of the message.</param>
    public async Task AddMessageAsync<T>(T message, string type)
    {
        var payload = JsonSerializer.Serialize(message);
        var outboxMessage = new OutboxMessage
        {
            Type = type,
            Content = payload
        };

        // Add the outbox message to the database context.
        await _dbContext.Set<OutboxMessage>().AddAsync(outboxMessage);
    }
}