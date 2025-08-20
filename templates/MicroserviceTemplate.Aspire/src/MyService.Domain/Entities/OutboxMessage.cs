namespace MyService.Domain.Entities;

public class OutboxMessage : BaseEntity
{
    public string Type { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime? ProcessedAt { get; set; }
}