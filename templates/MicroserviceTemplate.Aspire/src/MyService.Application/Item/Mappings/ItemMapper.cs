using Riok.Mapperly.Abstractions;

namespace MyService.Application.Item.Mappings;

[Mapper]
public partial class ItemMapper : IItemMapper
{
    public partial Dtos.ItemDto ToDto(Domain.Entities.Item item);


    [MapperIgnoreTarget(nameof(Domain.Entities.Item.Id))]
    [MapperIgnoreTarget(nameof(Domain.Entities.Item.CreatedAt))]
    [MapperIgnoreTarget(nameof(Domain.Entities.Item.UpdatedAt))]
    public partial Domain.Entities.Item ToEntity(Dtos.CreateItemDto dto);

    public partial IEnumerable<Dtos.ItemDto> ToDto(IEnumerable<Domain.Entities.Item> items);
}


