using MyService.Domain.Common.Entities;

namespace MyService.Domain.Items.Entities;

public class Item : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
