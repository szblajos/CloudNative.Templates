using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using MyService.Application.Common;
using MyService.Application.Items.Dtos;
using MyService.Domain.Items.Entities;
using System.Text.Json;

namespace MyService.Tests.Items.EndpointTests;

public class PaginationEndpointTests : EndpointsTestsBase
{
    public PaginationEndpointTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task GetItems_WithDefaultPaging_ReturnsPagedResult()
    {
        // Arrange
        var itemDtos = GenerateTestItemDtos(10); // First 10 items for page 1
        var totalCount = 25;

        ItemRepositoryMock.Setup(repo => repo.GetPagedProjectionAsync<ItemDto>(
                It.IsAny<Func<IQueryable<Item>, IQueryable<ItemDto>>>(),
                1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((itemDtos, totalCount));

        CacheMock.Setup(x => x.GetResponseAsync<PagedResult<ItemDto>>(It.IsAny<string>()))
                  .ReturnsAsync((PagedResult<ItemDto>?)null);

        // Act
        var response = await Client.GetAsync("/api/items");

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

    // ... további lapozási tesztek
}