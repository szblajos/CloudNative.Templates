using MyService.Application.Common;
using MyService.Application.Items.Dtos;
using Mediator;

namespace MyService.Application.Items.Queries;

public record GetItemsQuery(PagingParameters? PagingParameters = null) : IRequest<PagedResult<ItemDto>>;