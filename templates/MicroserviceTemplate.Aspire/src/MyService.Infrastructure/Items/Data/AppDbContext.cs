using Microsoft.EntityFrameworkCore;
using MyService.Domain.Items.Entities;

namespace MyService.Infrastructure.Data;

/// <summary>
/// Application database context. (Items related part)
/// </summary>
public partial class AppDbContext
{
    /// <summary>
    /// Gets or sets the items.
    /// </summary>
    public DbSet<Item> Items => Set<Item>();

    /// <summary>
        /// Configures the model.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                // Additional configurations if needed
            });
        }
}