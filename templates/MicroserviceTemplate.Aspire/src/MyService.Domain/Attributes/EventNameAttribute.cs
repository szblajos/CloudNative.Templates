namespace MyService.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class EventNameAttribute : Attribute
{
    public string Name { get; }
    public EventNameAttribute(string name) => Name = name;
}
