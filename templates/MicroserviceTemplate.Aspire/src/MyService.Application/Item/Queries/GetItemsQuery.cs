using MyService.Application.Common;
using MyService.Application.Item.Dtos;
using Mediator;

namespace MyService.Application.Item.Queries;

public record GetItemsQuery(PagingParameters? PagingParameters = null) : IRequest<PagedResult<ItemDto>>;