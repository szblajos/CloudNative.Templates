using Mediator;
using MyService.Application.Common;
using MyService.Application.Items.Dtos;
using MyService.Application.Items.Mappings;
using MyService.Application.Items.Queries;
using MyService.Domain.Items.Interfaces;

namespace MyService.Application.Items.Handlers;

public class GetItemsHandler(IItemsRepository itemsRepository, IItemMapper mapper) : IRequestHandler<GetItemsQuery, PagedResult<ItemDto>>
{
    private readonly IItemsRepository _itemsRepository = itemsRepository;
    private readonly IItemMapper _mapper = mapper;
    public async ValueTask<PagedResult<ItemDto>> Handle(GetItemsQuery query, CancellationToken cancellationToken)
    {
        if (query.PagingParameters != null)
        {
            var (itemDtos, totalCount) = await _itemsRepository.GetPagedProjectionAsync(
                _mapper.ProjectToDto,
                query.PagingParameters.PageNumber,
                query.PagingParameters.PageSize,
                cancellationToken);
                
            return new PagedResult<ItemDto>(itemDtos, totalCount, query.PagingParameters.PageNumber, query.PagingParameters.PageSize);
        }
        else
        {
            var itemDtos = await _itemsRepository.GetAllProjectionAsync(
                _mapper.ProjectToDto,
                cancellationToken);
                
            return new PagedResult<ItemDto>(itemDtos, itemDtos.Count());
        }
    }
}
