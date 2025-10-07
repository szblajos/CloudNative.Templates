using Riok.Mapperly.Abstractions;

namespace MyService.Application.Item.Mappings;

[Mapper]
public static partial class ItemMapper
{
    public static partial Dtos.ItemDto ToDto(Domain.Entities.Item item);

    [MapperIgnoreTarget(nameof(Domain.Entities.Item.Id))]
    [MapperIgnoreTarget(nameof(Domain.Entities.Item.CreatedAt))]
    [MapperIgnoreTarget(nameof(Domain.Entities.Item.UpdatedAt))]
    public static partial Domain.Entities.Item ToEntity(Dtos.CreateItemDto dto);

    public static partial IEnumerable<Dtos.ItemDto> ToDto(IEnumerable<Domain.Entities.Item> items);

    public static partial IQueryable<Dtos.ItemDto> ProjectToDto(IQueryable<Domain.Entities.Item> items);   
}


