namespace MyService.Application.Items.Dtos;

public class CreateItemDto
{
    public required string Name { get; set; }
    public required int Quantity { get; set; }
}
