using Mediator;
using MyService.Application.Common;
using MyService.Application.Item.Dtos;
using MyService.Application.Item.Mappings;
using MyService.Application.Item.Queries;
using MyService.Domain.Interfaces;

namespace MyService.Application.Item.Handlers;

public class GetItemsHandler(IItemRepository itemRepository, IItemMapper mapper) : IRequestHandler<GetItemsQuery, PagedResult<ItemDto>>
{
    private readonly IItemRepository _itemRepository = itemRepository;
    private readonly IItemMapper _mapper = mapper;
    public async ValueTask<PagedResult<ItemDto>> Handle(GetItemsQuery query, CancellationToken cancellationToken)
    {
        if (query.PagingParameters != null)
        {
            var totalCount = await _itemRepository.GetCountAsync(cancellationToken);
            var items = await _itemRepository.GetPagedAsync(query.PagingParameters.PageNumber, query.PagingParameters.PageSize, cancellationToken);
            var itemDtos = _mapper.ToDto(items);
            return new PagedResult<ItemDto>(itemDtos, totalCount, query.PagingParameters.PageNumber, query.PagingParameters.PageSize);
        }
        else
        {
            var items = await _itemRepository.GetAllAsync();
            var itemDtos = _mapper.ToDto(items);
            return new PagedResult<ItemDto>(itemDtos.ToArray(), items.Count());
        }
    }
}
