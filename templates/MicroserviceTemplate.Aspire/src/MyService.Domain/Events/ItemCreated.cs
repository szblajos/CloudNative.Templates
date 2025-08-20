namespace MyService.Domain.Events;

public class ItemCreatedV1
{
    public int ItemId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
