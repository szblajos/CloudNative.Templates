using Mediator;
using MyService.Application.Items.Dtos;

namespace MyService.Application.Items.Queries;

public class GetItemsByIdQuery : IRequest<ItemDto>
{
    public int Id { get; }

    public GetItemsByIdQuery(int id)
    {
        Id = id;
    }
}
