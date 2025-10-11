using Riok.Mapperly.Abstractions;

namespace MyService.Application.Item.Mappings;

[Mapper]
public partial class ItemMapper : IItemMapper
{
    public partial Dtos.ItemDto ToDto(Domain.Items.Entities.Item item);

    [MapperIgnoreTarget(nameof(Domain.Items.Entities.Item.Id))]
    [MapperIgnoreTarget(nameof(Domain.Items.Entities.Item.CreatedAt))]
    [MapperIgnoreTarget(nameof(Domain.Items.Entities.Item.UpdatedAt))]
    public partial Domain.Items.Entities.Item ToEntity(Dtos.CreateItemDto dto);

    public partial IEnumerable<Dtos.ItemDto> ToDto(IEnumerable<Domain.Items.Entities.Item> items);

    public partial IQueryable<Dtos.ItemDto> ProjectToDto(IQueryable<Domain.Items.Entities.Item> items);

}


