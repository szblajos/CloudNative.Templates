using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using MyService.Application.Item.Dtos;
using MyService.Domain.Items.Entities;
using System.Net;
using System.Net.Http.Json;

namespace MyService.Tests.Items.EndpointTests;

public class CreateItemEndpointTests : EndpointsTestsBase
{
    public CreateItemEndpointTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task CreateItem_ReturnsCreated()
    {
        // Arrange
        var newItem = new ItemDto { Name = "New Item" };
        ItemRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask);

        // Act
        var response = await Client.PostAsJsonAsync("/api/items", newItem);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("New Item", content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData("Item with negative quantity", -1)]
    [InlineData("", 5)] // Empty name
    [InlineData("Item with a very long name that exceeds the maximum length aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", 5)] // Name too long
    public async Task CreateItem_ReturnsBadRequest_WhenItemIsInvalid(string itemName, int quantity)
    {
        // Arrange
        var newItem = new ItemDto { Name = itemName, Quantity = quantity };

        // Act
        var response = await Client.PostAsJsonAsync("/api/items", newItem);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("Item with negative quantity", -1)]
    [InlineData("", 5)] // Empty name
    [InlineData("Item with a very long name that exceeds the maximum length aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", 5)] // Name too long
    public async Task CreateItem_ReturnsErrorDescription_WhenItemIsInvalid(string itemName, int quantity)
    {
        // Arrange
        var newItem = new ItemDto { Name = itemName, Quantity = quantity };

        // Act
        var response = await Client.PostAsJsonAsync("/api/items", newItem);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("error", content);
    }
}