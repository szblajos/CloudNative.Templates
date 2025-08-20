using Mediator;
using MyService.Application.Item.Commands;
using MyService.Application.Item.Mappings;
using MyService.Domain.Interfaces;
using MyService.Application.Item.Dtos;
using FluentValidation;
using MyService.Application.Item.Validations;
using System;
using MyService.Domain.Events;

namespace MyService.Application.Item.Handlers;

public sealed class CreateItemHandler(
    IItemRepository repository,
    IItemMapper mapper,
    IUnitOfWork unitOfWork,
    IValidator<CreateItemCommand> validator) : IRequestHandler<CreateItemCommand, ItemDto>
{
    private readonly IItemRepository _repository = repository;
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

        // Start the unit of work
        await _unitOfWork.BeginAsync();

        // Add the entity
        await _repository.AddAsync(entity);

        // Publish the event
        var evt = new ItemCreatedV1 { ItemId = entity.Id };
        await _unitOfWork.PublishDomainEventAsync(evt, nameof(ItemCreatedV1));

        // Commit the transaction
        await _unitOfWork.CommitAsync();

        return _mapper.ToDto(entity);
    }
}
