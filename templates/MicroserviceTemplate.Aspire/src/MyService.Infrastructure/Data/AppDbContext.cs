using Microsoft.EntityFrameworkCore;
using MyService.Domain.Items.Entities;
using MyService.Domain.Common.Entities;

namespace MyService.Infrastructure.Data;

/// <summary>
/// Application database context. (Common part)
/// </summary>
public partial class AppDbContext : DbContext
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
    /// Configures the model.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <remarks>
    /// This method is intended to be implemented in other partial class files to configure additional entities.
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Call the partial method to configure additional entities.
        OnModelCreatingPartial(modelBuilder);

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ProcessedAt).IsRequired(false);
        });
    }

    /// <summary>
    /// Partial method for additional model configuration.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <remarks>
    /// This method is intended to be implemented in other partial class files to configure additional entities.
    /// </remarks>
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

