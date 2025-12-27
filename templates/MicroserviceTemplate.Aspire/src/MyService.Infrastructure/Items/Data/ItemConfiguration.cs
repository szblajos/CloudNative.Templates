using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyService.Domain.Items.Entities;

namespace MyService.Infrastructure.Items.Data;

/// <summary>
/// Entity configuration for Item.
/// </summary>
public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    /// <summary>
    /// Configures the Item entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        // Additional configurations if needed
    }
}
