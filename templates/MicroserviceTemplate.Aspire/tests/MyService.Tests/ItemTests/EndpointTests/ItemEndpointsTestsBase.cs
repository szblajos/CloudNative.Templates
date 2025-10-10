using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MyService.Application.Common;
using MyService.Application.Item.Dtos;
using MyService.Domain.Entities;
using MyService.Domain.Interfaces;
using StackExchange.Redis;
using System.Net.Http;

namespace MyService.Tests.ItemTests.EndpointTests;

public abstract class ItemEndpointsTestsBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly HttpClient Client;
    protected readonly Mock<IItemRepository> ItemRepositoryMock = new();
    protected readonly Mock<IOutboxMessageRepository> OutboxMessageRepositoryMock = new();
    protected readonly Mock<ICacheService> CacheMock = new();
    protected readonly Mock<IMessagePublisher> MessagePublisherMock = new();
    protected readonly Mock<IUnitOfWork> UnitOfWorkMock = new();
    protected readonly Mock<IConnectionMultiplexer> ConnectionMultiplexerMock = new();

    protected ItemEndpointsTestsBase(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");

        Client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing service registrations
                RemoveService<IItemRepository>(services);
                RemoveService<IOutboxMessageRepository>(services);
                RemoveService<IConnectionMultiplexer>(services);
                RemoveService<ICacheService>(services);
                RemoveService<IMessagePublisher>(services);
                RemoveService<IUnitOfWork>(services);

                // Add the mocked services
                services.AddSingleton(ItemRepositoryMock.Object);
                services.AddSingleton(OutboxMessageRepositoryMock.Object);
                services.AddSingleton(ConnectionMultiplexerMock.Object);
                services.AddSingleton(CacheMock.Object);
                services.AddSingleton(MessagePublisherMock.Object);
                services.AddSingleton(UnitOfWorkMock.Object);

                // Setup OutboxMessageRepository mock for hosted service
                OutboxMessageRepositoryMock.Setup(repo => repo.GetUnprocessedMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<OutboxMessage>());

                OutboxMessageRepositoryMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
            });
        }).CreateClient();
    }

    private static void RemoveService<T>(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
    }

    protected static List<ItemDto> GenerateTestItemDtos(int count)
    {
        var itemDtos = new List<ItemDto>();
        for (int i = 1; i <= count; i++)
        {
            itemDtos.Add(new ItemDto
            {
                Id = i,
                Name = $"Test Item {i}",
                Quantity = i * 10
            });
        }
        return itemDtos;
    }
}