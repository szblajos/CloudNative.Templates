namespace MyService.Domain.Items.Events;

public class ItemDeletedV1
{
    public int ItemId { get; set; }
    public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
}
