using Microsoft.EntityFrameworkCore;
using MyService.Domain.Entities;

namespace MyService.Infrastructure.Data;

/// <summary>
/// Application database context.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the outbox messages.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = default!;

    /// <summary>
    /// Gets or sets the items.
    /// </summary>
    public DbSet<Item> Items => Set<Item>();

    /// <summary>
    /// Configures the model.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Additional configurations if needed
    }
}

