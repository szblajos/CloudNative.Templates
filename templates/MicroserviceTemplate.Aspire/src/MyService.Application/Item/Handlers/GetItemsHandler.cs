using Mediator;
using MyService.Application.Item.Dtos;
using MyService.Application.Item.Mappings;
using MyService.Application.Item.Queries;
using MyService.Domain.Interfaces;

namespace MyService.Application.Item.Handlers;

public class GetItemsHandler(IItemRepository itemRepository, IItemMapper mapper) : IRequestHandler<GetItemsQuery, ItemDto[]>
{
    private readonly IItemRepository _itemRepository = itemRepository;
    private readonly IItemMapper _mapper = mapper;

    async ValueTask<ItemDto[]> IRequestHandler<GetItemsQuery, ItemDto[]>.Handle(GetItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _itemRepository.GetAllAsync();
        return [.. _mapper.ToDto(items)];
    }
}
