using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MyService.Application.Item.Dtos;
using MyService.Application.Item.Mappings;
using MyService.Domain.Entities;
using MyService.Domain.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using Xunit;

namespace MyService.Tests;

public class ItemEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Mock<IItemRepository> _itemRepositoryMock = new();
    private readonly Mock<IOutboxMessageRepository> _outboxMessageRepositoryMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly Mock<IMessagePublisher> _messagePublisherMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IConnectionMultiplexer> _connectionMultiplexerMock = new();

    public ItemEndpointsTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");

        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing IItemRepository registrations
                var itemRepositoryDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IItemRepository));

                if (itemRepositoryDescriptor != null)
                {
                    services.Remove(itemRepositoryDescriptor);
                }

                // Remove the existing IOutboxMessageRepository registrations
                var outboxMessageRepositoryDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IOutboxMessageRepository));

                if (outboxMessageRepositoryDescriptor != null)
                {
                    services.Remove(outboxMessageRepositoryDescriptor);
                }

                // Remove the existing IConnectionMultiplexer registrations
                var connectionMultiplexerDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IConnectionMultiplexer));

                if (connectionMultiplexerDescriptor != null)
                {
                    services.Remove(connectionMultiplexerDescriptor);
                }

                // Remove the existing ICacheService registrations
                var cacheDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ICacheService));

                if (cacheDescriptor != null)
                {
                    services.Remove(cacheDescriptor);
                }

                // Remove the existing IMessagePublisher registrations
                var messagePublisherDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IMessagePublisher));

                if (messagePublisherDescriptor != null)
                {
                    services.Remove(messagePublisherDescriptor);
                }

                // Remove the existing IUnitOfWork registrations
                var unitOfWorkDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IUnitOfWork));

                if (unitOfWorkDescriptor != null)
                {
                    services.Remove(unitOfWorkDescriptor);
                }

                // Add the mocked services
                services.AddSingleton(_itemRepositoryMock.Object);
                services.AddSingleton(_outboxMessageRepositoryMock.Object);
                services.AddSingleton(_connectionMultiplexerMock.Object);
                services.AddSingleton(_cacheMock.Object);
                services.AddSingleton(_messagePublisherMock.Object);
                services.AddSingleton(_unitOfWorkMock.Object);

                // We must setup the OutboxMessageRepository mock here, for the hosted service, that starts along with the application
                _outboxMessageRepositoryMock.Setup(repo => repo.GetUnprocessedMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<OutboxMessage>());

                _outboxMessageRepositoryMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetItems_UsesCacheService()
    {
        // Arrange
        _itemRepositoryMock.Setup(repo => repo.GetAllAsync())
                           .ReturnsAsync(new List<Item>());

        _cacheMock.Setup(x => x.GetResponseAsync<IEnumerable<ItemDto>?>(It.IsAny<string>()))
                  .ReturnsAsync((IEnumerable<ItemDto>?)new List<ItemDto> { new ItemDto { Id = 1, Name = "Test Item" } });

        // Act
        var response = await _client.GetAsync("/items");

        // Assert
        _cacheMock.Verify(x => x.GetResponseAsync<IEnumerable<ItemDto>?>(It.IsAny<string>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task GetItems_ReturnsOk()
    {
        // Arrange
        _itemRepositoryMock.Setup(repo => repo.GetAllAsync())
                           .ReturnsAsync(new List<Item> { new Item { Id = 1, Name = "Test Item" } });

        _cacheMock.Setup(x => x.GetResponseAsync<IEnumerable<ItemDto>?>(It.IsAny<string>()))
                  .ReturnsAsync((IEnumerable<ItemDto>?)null);

        // Act
        var response = await _client.GetAsync("/items");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Test Item", content);
    }

    [Fact]
    public async Task GetItemById_ReturnsOk_WhenItemExists()
    {
        // Arrange
        var itemId = 1;
        _itemRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId))
                           .ReturnsAsync(new Item { Id = itemId, Name = "Test Item" });

        // Act
        var response = await _client.GetAsync($"/items/{itemId}");

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
        _itemRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId))
                           .ReturnsAsync((Item?)null);

        // Act
        var response = await _client.GetAsync($"/items/{itemId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateItem_ReturnsCreated()
    {
        // Arrange
        var newItem = new ItemDto { Name = "New Item" };
        _itemRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Item>()))
                           .Returns(Task.CompletedTask);

        // Act
        var response = await _client.PostAsJsonAsync("/items", newItem);

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
        var response = await _client.PostAsJsonAsync("/items", newItem);

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
        var response = await _client.PostAsJsonAsync("/items", newItem);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("error", content);
    }

    [Fact]
    public async Task UpdateItem_ReturnsNoContent_WhenItemIsValid()
    {
        // Arrange
        var itemId = 1;
        var existingItem = new Item { Id = itemId, Name = "Old Item", Quantity = 10 };
        var updatedItemDto = new ItemDto { Name = "Updated Item", Quantity = 20 };

        _itemRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId))
                           .ReturnsAsync(existingItem);
        _itemRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Item>()))
                           .Returns(Task.CompletedTask);

        // Act
        var response = await _client.PutAsJsonAsync($"/items/{itemId}", updatedItemDto);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Theory]
    [InlineData("Item with negative quantity", -1)]
    [InlineData("", 5)] // Empty name
    [InlineData("Item with a very long name that exceeds the maximum length aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", 5)] // Name too
    public async Task UpdateItem_ReturnsBadRequest_WhenItemIsInvalid(string itemName, int quantity)
    {
        // Arrange
        var itemId = 1;
        var updatedItemDto = new ItemDto { Name = itemName, Quantity = quantity };

        // Act
        var response = await _client.PutAsJsonAsync($"/items/{itemId}", updatedItemDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateItem_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        var itemId = 1;
        var updatedItemDto = new ItemDto { Name = "Updated Item", Quantity = 20 };

        _itemRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId))
                           .ReturnsAsync((Item?)null);

        // Act
        var response = await _client.PutAsJsonAsync($"/items/{itemId}", updatedItemDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteItem_ReturnsNoContent_WhenItemExists()
    {
        // Arrange
        var itemId = 1;
        var item = new Item { Id = itemId, Name = "Test Item" };
        _itemRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId))
                           .ReturnsAsync(item);
        _itemRepositoryMock.Setup(repo => repo.DeleteAsync(item))
                           .Returns(Task.CompletedTask);

        // Act
        var response = await _client.DeleteAsync($"/items/{itemId}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteItem_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        var itemId = 1;
        _itemRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId))
                           .ReturnsAsync((Item?)null);

        // Act
        var response = await _client.DeleteAsync($"/items/{itemId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}