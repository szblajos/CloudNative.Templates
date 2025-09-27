using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MyService.Application.Common;
using MyService.Application.Item.Dtos;
using MyService.Domain.Entities;
using MyService.Domain.Interfaces;
using StackExchange.Redis;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;


namespace MyService.Tests.ItemTests;

public class ItemEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    #region Fields

    private readonly HttpClient _client;
    private readonly Mock<IItemRepository> _itemRepositoryMock = new();
    private readonly Mock<IOutboxMessageRepository> _outboxMessageRepositoryMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly Mock<IMessagePublisher> _messagePublisherMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IConnectionMultiplexer> _connectionMultiplexerMock = new();

    #endregion

    #region Constructor

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

    #endregion

    #region GetItems Tests

    [Fact]
    public async Task GetItems_UsesCacheService()
    {
        // Arrange
        _itemRepositoryMock.Setup(repo => repo.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(new List<Item>());

        _cacheMock.Setup(x => x.GetResponseAsync<PagedResult<ItemDto>?>(It.IsAny<string>()))
                  .ReturnsAsync((PagedResult<ItemDto>?)new PagedResult<ItemDto> { Items = new List<ItemDto> { new ItemDto { Id = 1, Name = "Test Item" } } });

        // Act
        var response = await _client.GetAsync("/items");

        // Assert
        _cacheMock.Verify(x => x.GetResponseAsync<PagedResult<ItemDto>?>(It.IsAny<string>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task GetItems_ReturnsOk()
    {
        // Arrange
        _itemRepositoryMock.Setup(repo => repo.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(new List<Item> { new Item { Id = 1, Name = "Test Item" } });

        _cacheMock.Setup(x => x.GetResponseAsync<PagedResult<ItemDto>?>("items:page:1:size:10"))
                  .ReturnsAsync((PagedResult<ItemDto>?)null);
                  
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

    #endregion

    #region CreateItem Tests

    [Fact]
    public async Task CreateItem_ReturnsCreated()
    {
        // Arrange
        var newItem = new ItemDto { Name = "New Item" };
        _itemRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
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

    #endregion

    #region UpdateItem Tests

    [Fact]
    public async Task UpdateItem_ReturnsNoContent_WhenItemIsValid()
    {
        // Arrange
        var itemId = 1;
        var existingItem = new Item { Id = itemId, Name = "Old Item", Quantity = 10 };
        var updatedItemDto = new ItemDto { Name = "Updated Item", Quantity = 20 };

        _itemRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId))
                           .ReturnsAsync(existingItem);
        _itemRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Item>(), It.IsAny<CancellationToken>()))
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
    [InlineData("Item with a very long name that exceeds the maximum length aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", 5)] // Name too long
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

    #endregion

    #region DeleteItem Tests

    [Fact]
    public async Task DeleteItem_ReturnsNoContent_WhenItemExists()
    {
        // Arrange
        var itemId = 1;
        var item = new Item { Id = itemId, Name = "Test Item" };
        _itemRepositoryMock.Setup(repo => repo.GetByIdAsync(itemId))
                           .ReturnsAsync(item);
        _itemRepositoryMock.Setup(repo => repo.DeleteAsync(item, It.IsAny<CancellationToken>()))
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

    #endregion

    #region Pagination Tests

    [Fact]
    public async Task GetItems_WithDefaultPaging_ReturnsPagedResult()
    {
        // Arrange
        var items = GenerateTestItems(25);
        var pagedItems = items.Take(10); // Default page size is 10

        _itemRepositoryMock.Setup(repo => repo.GetCountAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(25);
        _itemRepositoryMock.Setup(repo => repo.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(pagedItems);

        _cacheMock.Setup(x => x.GetResponseAsync<PagedResult<ItemDto>>(It.IsAny<string>()))
                  .ReturnsAsync((PagedResult<ItemDto>?)null);

        // Act
        var response = await _client.GetAsync("/items");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<ItemDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(pagedResult);
        Assert.Equal(10, pagedResult.Items?.Count());
        Assert.Equal(25, pagedResult.TotalCount);
        Assert.Equal(1, pagedResult.PageNumber);
        Assert.Equal(10, pagedResult.PageSize);
        Assert.Equal(3, pagedResult.TotalPages);
        Assert.False(pagedResult.HasPreviousPage);
        Assert.True(pagedResult.HasNextPage);
    }

    [Fact]
    public async Task GetItems_WithCustomPaging_ReturnsCorrectPage()
    {
        // Arrange
        var items = GenerateTestItems(25);
        var pagedItems = items.Skip(10).Take(5); // Page 3, size 5

        _itemRepositoryMock.Setup(repo => repo.GetCountAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(25);
        _itemRepositoryMock.Setup(repo => repo.GetPagedAsync(3, 5, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(pagedItems);

        _cacheMock.Setup(x => x.GetResponseAsync<PagedResult<ItemDto>>(It.IsAny<string>()))
                  .ReturnsAsync((PagedResult<ItemDto>?)null);

        // Act
        var response = await _client.GetAsync("/items?pageNumber=3&pageSize=5");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<ItemDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(pagedResult);
        Assert.Equal(5, pagedResult.Items?.Count());
        Assert.Equal(25, pagedResult.TotalCount);
        Assert.Equal(3, pagedResult.PageNumber);
        Assert.Equal(5, pagedResult.PageSize);
        Assert.Equal(5, pagedResult.TotalPages); // 25 items / 5 per page
        Assert.True(pagedResult.HasPreviousPage);
        Assert.True(pagedResult.HasNextPage);
    }

    [Fact]
    public async Task GetItems_WithLastPage_HasNoNextPage()
    {
        // Arrange
        var items = GenerateTestItems(23);
        var pagedItems = items.Skip(20).Take(10); // Last page with 3 items

        _itemRepositoryMock.Setup(repo => repo.GetCountAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(23);
        _itemRepositoryMock.Setup(repo => repo.GetPagedAsync(3, 10, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(pagedItems);

        _cacheMock.Setup(x => x.GetResponseAsync<PagedResult<ItemDto>>(It.IsAny<string>()))
                  .ReturnsAsync((PagedResult<ItemDto>?)null);

        // Act
        var response = await _client.GetAsync("/items?pageNumber=3&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<ItemDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(pagedResult);
        Assert.Equal(3, pagedResult.Items?.Count());
        Assert.Equal(23, pagedResult.TotalCount);
        Assert.Equal(3, pagedResult.PageNumber);
        Assert.Equal(10, pagedResult.PageSize);
        Assert.Equal(3, pagedResult.TotalPages);
        Assert.True(pagedResult.HasPreviousPage);
        Assert.False(pagedResult.HasNextPage);
    }

    [Fact]
    public async Task GetItems_WithEmptyResult_ReturnsEmptyPagedResult()
    {
        // Arrange
        _itemRepositoryMock.Setup(repo => repo.GetCountAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(0);
        _itemRepositoryMock.Setup(repo => repo.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(Enumerable.Empty<Item>());

        _cacheMock.Setup(x => x.GetResponseAsync<PagedResult<ItemDto>>(It.IsAny<string>()))
                  .ReturnsAsync((PagedResult<ItemDto>?)null);

        // Act
        var response = await _client.GetAsync("/items");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<ItemDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(pagedResult);
        Assert.Empty(pagedResult.Items ?? Enumerable.Empty<ItemDto>());
        Assert.Equal(0, pagedResult.TotalCount);
        Assert.Equal(1, pagedResult.PageNumber);
        Assert.Equal(10, pagedResult.PageSize);
        Assert.Equal(0, pagedResult.TotalPages);
        Assert.False(pagedResult.HasPreviousPage);
        Assert.False(pagedResult.HasNextPage);
    }

    [Theory]
    [InlineData(0, 10, 1, 10)] // pageNumber 0 should default to 1
    [InlineData(-1, 10, 1, 10)] // negative pageNumber should default to 1
    [InlineData(1, 0, 1, 10)] // pageSize 0 should default to 10
    [InlineData(1, -5, 1, 10)] // negative pageSize should default to 10
    public async Task GetItems_WithInvalidPagingParameters_UsesDefaults(int inputPageNumber, int inputPageSize, int expectedPageNumber, int expectedPageSize)
    {
        // Arrange
        var items = GenerateTestItems(15);
        var pagedItems = items.Take(expectedPageSize);

        _itemRepositoryMock.Setup(repo => repo.GetCountAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(15);
        _itemRepositoryMock.Setup(repo => repo.GetPagedAsync(expectedPageNumber, expectedPageSize, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(pagedItems);

        _cacheMock.Setup(x => x.GetResponseAsync<PagedResult<ItemDto>>(It.IsAny<string>()))
                  .ReturnsAsync((PagedResult<ItemDto>?)null);

        // Act
        var response = await _client.GetAsync($"/items?pageNumber={inputPageNumber}&pageSize={inputPageSize}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<ItemDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(pagedResult);
        Assert.Equal(expectedPageNumber, pagedResult.PageNumber);
        Assert.Equal(expectedPageSize, pagedResult.PageSize);
    }

    [Fact]
    public async Task GetItems_WithPageSizeExceedingMaximum_CapsAtMaximum()
    {
        // Arrange
        var items = GenerateTestItems(150);
        var pagedItems = items.Take(100); // Should be capped at 100

        _itemRepositoryMock.Setup(repo => repo.GetCountAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(150);
        _itemRepositoryMock.Setup(repo => repo.GetPagedAsync(1, 100, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(pagedItems);

        _cacheMock.Setup(x => x.GetResponseAsync<PagedResult<ItemDto>>(It.IsAny<string>()))
                  .ReturnsAsync((PagedResult<ItemDto>?)null);

        // Act
        var response = await _client.GetAsync("/items?pageSize=150");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<ItemDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(pagedResult);
        Assert.Equal(100, pagedResult.PageSize); // Should be capped at maximum
        Assert.Equal(100, pagedResult.Items?.Count());
    }

    [Fact]
    public async Task GetItems_WithPaging_UsesCacheWithCorrectKey()
    {
        // Arrange
        var cachedResult = new PagedResult<ItemDto>
        {
            Items = new List<ItemDto> { new ItemDto { Id = 1, Name = "Cached Item" } },
            TotalCount = 1,
            PageNumber = 2,
            PageSize = 5
        };

        _cacheMock.Setup(x => x.GetResponseAsync<PagedResult<ItemDto>>("items:page:2:size:5"))
                  .ReturnsAsync(cachedResult);

        // Act
        var response = await _client.GetAsync("/items?pageNumber=2&pageSize=5");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        
        // Verify cache was checked with correct key
        _cacheMock.Verify(x => x.GetResponseAsync<PagedResult<ItemDto>>("items:page:2:size:5"), Times.Once);
        
        // Verify repository methods were NOT called since we got cached result
        _itemRepositoryMock.Verify(repo => repo.GetCountAsync(It.IsAny<CancellationToken>()), Times.Never);
        _itemRepositoryMock.Verify(repo => repo.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);

        Assert.Contains("Cached Item", content);
    }

    [Fact]
    public async Task GetItems_WithPaging_CachesResultAfterDatabaseCall()
    {
        // Arrange
        var items = GenerateTestItems(20);
        var pagedItems = items.Skip(5).Take(5); // Page 2, size 5

        _itemRepositoryMock.Setup(repo => repo.GetCountAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(20);
        _itemRepositoryMock.Setup(repo => repo.GetPagedAsync(2, 5, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(pagedItems);

        _cacheMock.Setup(x => x.GetResponseAsync<PagedResult<ItemDto>>(It.IsAny<string>()))
                  .ReturnsAsync((PagedResult<ItemDto>?)null);

        // Act
        var response = await _client.GetAsync("/items?pageNumber=2&pageSize=5");

        // Assert
        response.EnsureSuccessStatusCode();
        
        // Verify cache was set with correct key and expiry
        _cacheMock.Verify(x => x.SetResponseAsync(
            "items:page:2:size:5", 
            It.IsAny<PagedResult<ItemDto>>(), 
            TimeSpan.FromMinutes(5)), 
            Times.Once);
    }

    [Fact]
    public async Task GetItems_WithPageBeyondTotalPages_ReturnsEmptyPage()
    {
        // Arrange
        var items = GenerateTestItems(10);

        _itemRepositoryMock.Setup(repo => repo.GetCountAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(10);
        _itemRepositoryMock.Setup(repo => repo.GetPagedAsync(5, 10, It.IsAny<CancellationToken>())) // Page 5 when only 1 page exists
                          .ReturnsAsync(Enumerable.Empty<Item>());

        _cacheMock.Setup(x => x.GetResponseAsync<PagedResult<ItemDto>>(It.IsAny<string>()))
                  .ReturnsAsync((PagedResult<ItemDto>?)null);

        // Act
        var response = await _client.GetAsync("/items?pageNumber=5&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var pagedResult = JsonSerializer.Deserialize<PagedResult<ItemDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(pagedResult);
        Assert.Empty(pagedResult.Items ?? Enumerable.Empty<ItemDto>());
        Assert.Equal(10, pagedResult.TotalCount);
        Assert.Equal(5, pagedResult.PageNumber);
        Assert.Equal(10, pagedResult.PageSize);
        Assert.Equal(1, pagedResult.TotalPages);
        Assert.True(pagedResult.HasPreviousPage);
        Assert.False(pagedResult.HasNextPage);
    }

    #endregion

    
    #region Helper Methods

    private static List<Item> GenerateTestItems(int count)
    {
        var items = new List<Item>();
        for (int i = 1; i <= count; i++)
        {
            items.Add(new Item
            {
                Id = i,
                Name = $"Test Item {i}",
                Quantity = i * 10
            });
        }
        return items;
    }

    #endregion

}