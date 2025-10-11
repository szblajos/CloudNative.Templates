using Mediator;
using MyService.Application.Items.Dtos;

namespace MyService.Application.Items.Commands;

public class CreateItemCommand(CreateItemDto item) : IRequest<ItemDto>
{
    public CreateItemDto Item { get; set; } = item;
}



