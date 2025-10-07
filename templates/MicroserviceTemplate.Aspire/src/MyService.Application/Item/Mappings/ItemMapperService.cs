namespace MyService.Application.Item.Mappings;

// Wrapper class for dependency injection compatibility
public class ItemMapperService : IItemMapper
{
    public Dtos.ItemDto ToDto(Domain.Entities.Item item) => ItemMapper.ToDto(item);

    public Domain.Entities.Item ToEntity(Dtos.CreateItemDto dto) => ItemMapper.ToEntity(dto);

    public IEnumerable<Dtos.ItemDto> ToDto(IEnumerable<Domain.Entities.Item> items) => ItemMapper.ToDto(items);

    public IQueryable<Dtos.ItemDto> ProjectToDto(IQueryable<Domain.Entities.Item> items) => ItemMapper.ProjectToDto(items);
}