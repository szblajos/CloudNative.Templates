using Mediator;
using MyService.Application.Items.Commands;
using MyService.Application.Items.Mappings;
using MyService.Domain.Items.Interfaces;
using MyService.Domain.Common.Interfaces;
using MyService.Application.Items.Dtos;
using FluentValidation;
using MyService.Application.Items.Validations;
using MyService.Domain.Items.Events;

namespace MyService.Application.Items.Handlers;

public sealed class CreateItemHandler(
    IItemsRepository itemsRepository,
    IItemMapper mapper,
    IUnitOfWork unitOfWork,
    IValidator<CreateItemCommand> validator) : IRequestHandler<CreateItemCommand, ItemDto>
{
    private readonly IItemsRepository _itemsRepository = itemsRepository;
    private readonly IItemMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IValidator<CreateItemCommand> _validator = validator;

    public async ValueTask<ItemDto> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        // Validate the request using FluentValidation
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Map the DTO to the entity
        var entity = _mapper.ToEntity(request.Item);

        try
        {
            // Start the unit of work
            await _unitOfWork.BeginAsync();

            // Add the new item to the repository
            await _itemsRepository.AddAsync(entity);

            // Publish the event
            var evt = new ItemCreatedV1 { ItemId = entity.Id };
            await _unitOfWork.PublishDomainEventAsync(evt, nameof(ItemCreatedV1));

            // Commit the transaction
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        return _mapper.ToDto(entity);
    }
}
