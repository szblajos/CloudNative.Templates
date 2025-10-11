using Mediator;
using MyService.Application.Items.Dtos;
using MyService.Application.Items.Mappings;
using MyService.Application.Items.Queries;
using MyService.Domain.Items.Interfaces;

namespace MyService.Application.Items.Handlers;

public class GetItemsByIdHandler(IItemsRepository itemsRepository, IItemMapper mapper) : IRequestHandler<GetItemsByIdQuery, ItemDto?>
{
    private readonly IItemsRepository _itemsRepository = itemsRepository;
    private readonly IItemMapper _mapper = mapper;

    async ValueTask<ItemDto?> IRequestHandler<GetItemsByIdQuery, ItemDto?>.Handle(GetItemsByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _itemsRepository.GetByIdAsync(request.Id);
        return item != null ? _mapper.ToDto(item) : null;
    }
}
