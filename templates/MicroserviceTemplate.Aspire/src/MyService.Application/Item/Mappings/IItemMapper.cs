

namespace MyService.Application.Item.Mappings;

public interface IItemMapper
{
    Dtos.ItemDto ToDto(Domain.Entities.Item item);
    Domain.Entities.Item ToEntity(Dtos.CreateItemDto dto);
    IEnumerable<Dtos.ItemDto> ToDto(IEnumerable<Domain.Entities.Item> items);
}