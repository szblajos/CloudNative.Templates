using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using MyService.Domain.Entities;
using System.Net;

namespace MyService.Tests.ItemTests.EndpointTests;

public class DeleteItemEndpointTests : ItemEndpointsTestsBase
{
    public DeleteItemEndpointTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task DeleteItem_ReturnsNoContent_WhenItemExists()
    {
        // Arrange
        var itemId = 1;
        var item = new Item { Id = itemId, Name = "Test Item" };
        ItemRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId))
                           .ReturnsAsync(item);
        ItemRepositoryMock.Setup(repo => repo.DeleteAsync(item, It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);

        // Act
        var response = await Client.DeleteAsync($"/api/items/{itemId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteItem_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        var itemId = 1;
        ItemRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId))
                           .ReturnsAsync((Item?)null);

        // Act
        var response = await Client.DeleteAsync($"/api/items/{itemId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}