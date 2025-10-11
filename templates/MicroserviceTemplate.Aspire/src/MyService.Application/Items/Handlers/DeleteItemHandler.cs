using Mediator;
using MyService.Application.Items.Commands;
using MyService.Domain.Items.Events;
using MyService.Domain.Items.Interfaces;
using MyService.Domain.Common.Interfaces;

namespace MyService.Application.Items.Handlers;

public class DeleteItemHandler : ICommandHandler<DeleteItemCommand>
{
    private readonly IItemsRepository _itemsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteItemHandler(IItemsRepository itemsRepository, IUnitOfWork unitOfWork)
    {
        _itemsRepository = itemsRepository;
        _unitOfWork = unitOfWork;
    }

    async ValueTask<Unit> ICommandHandler<DeleteItemCommand, Unit>.Handle(DeleteItemCommand command, CancellationToken cancellationToken)
    {
        var item = await _itemsRepository.GetByIdAsync(command.Id);
        if (item is not null)
        {
            try
            {
                // Start the unit of work
                await _unitOfWork.BeginAsync();

                await _itemsRepository.DeleteAsync(item);

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
