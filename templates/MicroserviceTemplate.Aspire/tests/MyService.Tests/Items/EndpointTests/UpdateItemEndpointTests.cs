using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using MyService.Application.Item.Dtos;
using MyService.Domain.Items.Entities;
using System.Net;
using System.Net.Http.Json;

namespace MyService.Tests.Items.EndpointTests;

public class UpdateItemEndpointTests : EndpointsTestsBase
{
    public UpdateItemEndpointTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task UpdateItem_ReturnsNoContent_WhenItemIsValid()
    {
        // Arrange
        var itemId = 1;
        var existingItem = new Item { Id = itemId, Name = "Old Item", Quantity = 10 };
        var updatedItemDto = new ItemDto { Name = "Updated Item", Quantity = 20 };

        ItemRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId))
                           .ReturnsAsync(existingItem);
        ItemRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);

        // Act
        var response = await Client.PutAsJsonAsync($"/api/items/{itemId}", updatedItemDto);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Theory]
    [InlineData("Item with negative quantity", -1)]
    [InlineData("", 5)] // Empty name
    [InlineData("Item with a very long name that exceeds the maximum length aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", 5)] // Name too long
    public async Task UpdateItem_ReturnsBadRequest_WhenItemIsInvalid(string itemName, int quantity)
    {
        // Arrange
        var itemId = 1;
        var updatedItemDto = new ItemDto { Name = itemName, Quantity = quantity };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/items/{itemId}", updatedItemDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateItem_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        var itemId = 1;
        var updatedItemDto = new ItemDto { Name = "Updated Item", Quantity = 20 };

        ItemRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId))
                           .ReturnsAsync((Item?)null);

        // Act
        var response = await Client.PutAsJsonAsync($"/api/items/{itemId}", updatedItemDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}