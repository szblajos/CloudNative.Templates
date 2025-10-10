using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using MyService.Domain.Entities;
using System.Net;

namespace MyService.Tests.Items.EndpointTests;

public class GetItemByIdEndpointTests : EndpointsTestsBase
{
    public GetItemByIdEndpointTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetItemById_ReturnsOk_WhenItemExists()
    {
        // Arrange
        var itemId = 1;
        ItemRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId))
                           .ReturnsAsync(new Item { Id = itemId, Name = "Test Item" });

        // Act
        var response = await Client.GetAsync($"/api/items/{itemId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Test Item", content);
    }

    [Fact]
    public async Task GetItemById_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        var itemId = 1;
        ItemRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId))
                           .ReturnsAsync((Item?)null);

        // Act
        var response = await Client.GetAsync($"/api/items/{itemId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}