using FluentValidation;
using Mediator;
using MyService.Application.Item.Commands;
using MyService.Domain.Items.Events;
using MyService.Domain.Items.Interfaces;
using MyService.Domain.Common.Interfaces;

namespace MyService.Application.Item.Handlers;

public class UpdateItemHandler(IItemsRepository itemsRepository, IValidator<UpdateItemCommand> validator, IUnitOfWork unitOfWork) : ICommandHandler<UpdateItemCommand>
{
    private readonly IItemsRepository _repository = itemsRepository;
    private readonly IValidator<UpdateItemCommand> _validator = validator;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async ValueTask<Unit> Handle(UpdateItemCommand request, CancellationToken cancellationToken)
    {
        // Validate the request using FluentValidation
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var item = await _repository.GetByIdAsync(request.Id);
        if (item is not null)
        {
            try
            {
                // Start the unit of work
                await _unitOfWork.BeginAsync();

                item.Name = request.Dto.Name;
                item.Quantity = request.Dto.Quantity;
                item.UpdatedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(item);

                // Publish the event
                var evt = new ItemUpdatedV1 { ItemId = request.Id };
                await _unitOfWork.PublishDomainEventAsync(evt, nameof(ItemUpdatedV1));

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
            throw new KeyNotFoundException($"Item with ID {request.Id} not found.");
        }

        return Unit.Value;
    }
}