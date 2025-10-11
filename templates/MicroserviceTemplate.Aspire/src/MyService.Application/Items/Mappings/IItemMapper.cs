namespace MyService.Application.Items.Mappings;

public interface IItemMapper
{
    Dtos.ItemDto ToDto(Domain.Items.Entities.Item item);
    Domain.Items.Entities.Item ToEntity(Dtos.CreateItemDto dto);
    IEnumerable<Dtos.ItemDto> ToDto(IEnumerable<Domain.Items.Entities.Item> items);
    IQueryable<Dtos.ItemDto> ProjectToDto(IQueryable<Domain.Items.Entities.Item> items);
}