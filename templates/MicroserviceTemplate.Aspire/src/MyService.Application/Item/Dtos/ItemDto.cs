namespace MyService.Application.Item.Dtos;

public class ItemDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
