using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using MyService.Application.Common;
using MyService.Application.Item.Dtos;
using MyService.Domain.Entities;
using System.Text.Json;

namespace MyService.Tests.ItemTests.EndpointTests;

public class GetItemsEndpointTests : ItemEndpointsTestsBase
{
    public GetItemsEndpointTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetItems_UsesCacheService()
    {
        // Arrange
        ItemRepositoryMock.Setup(repo => repo.GetPagedProjectionAsync<ItemDto>(
                It.IsAny<Func<IQueryable<Item>, IQueryable<ItemDto>>>(),
                1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<ItemDto>(), 0));

        CacheMock.Setup(x => x.GetResponseAsync<PagedResult<ItemDto>?>(It.IsAny<string>()))
                  .ReturnsAsync((PagedResult<ItemDto>?)new PagedResult<ItemDto> { Items = new List<ItemDto> { new ItemDto { Id = 1, Name = "Test Item" } } });

        // Act
        var response = await Client.GetAsync("/api/items");

        // Assert
        CacheMock.Verify(x => x.GetResponseAsync<PagedResult<ItemDto>?>(It.IsAny<string>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task GetItems_ReturnsOk()
    {
        // Arrange
        var testItemDtos = new List<ItemDto> { new ItemDto { Id = 1, Name = "Test Item" } };
        
        ItemRepositoryMock.Setup(repo => repo.GetPagedProjectionAsync<ItemDto>(
                It.IsAny<Func<IQueryable<Item>, IQueryable<ItemDto>>>(),
                1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((testItemDtos, 1));

        CacheMock.Setup(x => x.GetResponseAsync<PagedResult<ItemDto>?>("items:page:1:size:10"))
                  .ReturnsAsync((PagedResult<ItemDto>?)null);
                  
        // Act
        var response = await Client.GetAsync("/api/items");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Test Item", content);
    }
}