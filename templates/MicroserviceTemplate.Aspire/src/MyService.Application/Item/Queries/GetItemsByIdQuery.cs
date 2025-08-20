using Mediator;
using MyService.Application.Item.Dtos;

namespace MyService.Application.Item.Queries;

public class GetItemsByIdQuery : IRequest<ItemDto>
{
    public int Id { get; }

    public GetItemsByIdQuery(int id)
    {
        Id = id;
    }
}
