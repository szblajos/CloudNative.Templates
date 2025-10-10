using MyService.Application.Item.Commands;
using MyService.Application.Item.Dtos;
using MyService.Application.Item.Validations;

namespace MyService.Tests.ItemTests.ValidationTests;

public class CreateItemValidationTests
{
    private readonly CreateItemCommandValidator _validator;

    public CreateItemValidationTests()
    {
        _validator = new CreateItemCommandValidator();
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenNameIsEmpty()
    {
        // Arrange
        var command = new CreateItemCommand(new CreateItemDto { Name = "", Quantity = 10 });

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Item.Name");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenQuantityIsNegative()
    {
        // Arrange
        var command = new CreateItemCommand(new CreateItemDto { Name = "Test Item", Quantity = -1 });

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Item.Quantity");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenNameExceedsMaxLength()
    {
        // Arrange
        var longName = new string('A', 101); // 101 characters
        var command = new CreateItemCommand(new CreateItemDto { Name = longName, Quantity = 10 });

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Item.Name");
    }

    [Fact]
    public void Validate_ShouldPass_WhenDataIsValid()
    {
        // Arrange
        var command = new CreateItemCommand(new CreateItemDto { Name = "Valid Item", Quantity = 5 });

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }
}