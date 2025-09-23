using Mediator;
using MyService.Application.Item.Commands;
using MyService.Domain.Events;
using MyService.Domain.Interfaces;

namespace MyService.Application.Item.Handlers;

public class DeleteItemHandler : ICommandHandler<DeleteItemCommand>
{
    private readonly IItemRepository _itemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteItemHandler(IItemRepository itemRepository, IUnitOfWork unitOfWork)
    {
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
    }

    async ValueTask<Unit> ICommandHandler<DeleteItemCommand, Unit>.Handle(DeleteItemCommand command, CancellationToken cancellationToken)
    {
        var item = await _itemRepository.GetByIdAsync(command.Id);
        if (item is not null)
        {
            try
            {
                // Start the unit of work
                await _unitOfWork.BeginAsync();

                await _itemRepository.DeleteAsync(item);

                // Publish the event
                var evt = new ItemDeletedV1 { ItemId = command.Id };
                await _unitOfWork.PublishDomainEventAsync(evt, nameof(ItemDeletedV1));

                // Commit the transaction
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        else
        {
            throw new KeyNotFoundException($"Item with ID {command.Id} not found.");
        }
        return Unit.Value;
    }
}
