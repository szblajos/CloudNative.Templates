namespace MyService.Domain.Events;

public class ItemUpdatedV1
{
    public int ItemId { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}