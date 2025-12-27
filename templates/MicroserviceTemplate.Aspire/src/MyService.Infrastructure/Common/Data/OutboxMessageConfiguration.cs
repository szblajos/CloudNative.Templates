using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyService.Domain.Common.Entities;

namespace MyService.Infrastructure.Common.Data;

/// <summary>
/// Entity configuration for OutboxMessage.
/// </summary>
public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    /// <summary>
    /// Configures the OutboxMessage entity.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Type).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Content).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.ProcessedAt).IsRequired(false);
    }
}
