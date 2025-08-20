using Mediator;
using MyService.Application.Item.Dtos;

namespace MyService.Application.Item.Queries;

public class GetItemsQuery : IRequest<ItemDto[]>
{
    public GetItemsQuery() { }
}
