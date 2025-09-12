namespace MyService.Application.Item.Dtos;

public class UpdateItemDto
{
    public string Name { get; set; } = default!;
    public int Quantity { get; set; }
    // Additional properties can be added as needed
}