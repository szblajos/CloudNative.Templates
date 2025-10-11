using Mediator;
using MyService.Application.Items.Dtos;

namespace MyService.Application.Items.Commands;

public class UpdateItemCommand : ICommand
{
    public int Id { get; set; }
    public UpdateItemDto Dto { get; set; } = default!;
}