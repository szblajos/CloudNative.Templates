

using Mediator;
using MyService.Application.Item.Dtos;

namespace MyService.Application.Item.Commands;

public class UpdateItemCommand : ICommand
{
    public int Id { get; set; }
    public UpdateItemDto Dto { get; set; } = default!;
}