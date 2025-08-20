using Mediator;
using MyService.Application.Item.Dtos;

namespace MyService.Application.Item.Commands;

public class CreateItemCommand(CreateItemDto item) : IRequest<ItemDto>
{
    public CreateItemDto Item { get; set; } = item;
}



