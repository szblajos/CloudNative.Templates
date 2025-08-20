using Mediator;

namespace MyService.Application.Item.Commands;

public class DeleteItemCommand : ICommand
{
    public int Id { get; }

    public DeleteItemCommand(int id)
    {
        Id = id;
    }
}
