namespace MyService.Infrastructure.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyService.Domain.Interfaces;
using MyService.Infrastructure.Cache;
using MyService.Infrastructure.Data;
using MyService.Infrastructure.Messaging;
using MyService.Infrastructure.Services;
using StackExchange.Redis;

/// <summary>
/// Extension methods for registering infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    
    // Helper method to check for test environment
    static bool IsTestEnvironment(IConfiguration config)
    {
        var environment = config["ASPNETCORE_ENVIRONMENT"];
        return string.Equals(environment, "Test", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(environment, "Testing", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(environment, "Integration", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Adds Azure Service Bus messaging services to the container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>Returns the configured service collection.</returns>
    private static IServiceCollection AddAzureServiceBus(this IServiceCollection services)
    {
        services.AddScoped<IMessagePublisher, AzureServiceBusPublisher>();
        return services;
    }

    /// <summary>
    /// Adds RabbitMQ messaging services to the container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>Returns the configured service collection.</returns>
    private static IServiceCollection AddRabbitMq(this IServiceCollection services)
    {
        services.AddScoped<IMessagePublisher, RabbitMqPublisher>();
        return services;
    }

    /// <summary>
    /// Adds message broker services to the container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The configuration.</param>
    /// <returns>Returns the configured service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the configuration is missing.</exception>
    /// <exception cref="ArgumentException">Thrown when the preferred transport is unsupported.</exception>
    public static IServiceCollection AddMessageBroker(this IServiceCollection services, IConfiguration config)
    {
        if (IsTestEnvironment(config))
        {
            // In test environment, we can use a mock or a test container for Messaging
            return services;
        }

        // Determine the preferred message broker transport
        var preferredTransport = config["MessageBroker:PreferredTransport"];

        // If not configured, throw an exception
        if (string.IsNullOrEmpty(preferredTransport))
        {
            throw new ArgumentNullException("MessageBroker:PreferredTransport configuration is missing");
        }

        // Register the preferred message broker
        if (preferredTransport == "AzureServiceBus")
        {
            services.AddAzureServiceBus();
        }
        else if (preferredTransport == "RabbitMQ")
        {
            services.AddRabbitMq();
        }
        else
        {
            throw new ArgumentException($"Unsupported message broker: {preferredTransport}");
        }

        return services;
    }

    /// <summary>
    /// Adds Redis caching services to the container.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">Thrown when the configuration is missing.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the connection to Redis fails.</exception>
    private static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration config)
    {

        var redisConnectionString = config["Cache:Redis:ConnectionString"] ?? throw new ArgumentNullException("Cache:Redis:ConnectionString configuration is missing");
        if (string.IsNullOrEmpty(redisConnectionString))
        {
            throw new ArgumentNullException("Redis:ConnectionString is not configured");
        }

        var redis = ConnectionMultiplexer.Connect(configuration: redisConnectionString);
        if (!redis.IsConnected)
        {
            // Handle Redis connection failure
            throw new InvalidOperationException("Failed to connect to Redis");
        }

        services.AddSingleton<IConnectionMultiplexer>(redis);
        services.AddSingleton<ICacheService, RedisCacheService>();
        return services;
    }

    /// <summary>
    /// Adds Azure Cache for Redis services to the container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The configuration.</param>
    /// <returns>Returns the configured service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the configuration is missing.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the connection to Redis fails.</exception>
    private static IServiceCollection AddAzureCacheForRedis(this IServiceCollection services, IConfiguration config)
    {
        var redisConnectionString = config["Cache:AzureCacheForRedis:ConnectionString"] ?? throw new ArgumentNullException("Cache:AzureCacheForRedis:ConnectionString configuration is missing");
        if (string.IsNullOrEmpty(redisConnectionString))
        {
            throw new ArgumentNullException("AzureCacheForRedis: connection string is not configured");
        }

        var redis = ConnectionMultiplexer.Connect(redisConnectionString);
        if (redis.IsConnected == false)
        {
            throw new InvalidOperationException("Failed to connect to Azure Cache for Redis");
        }

        services.AddSingleton<IConnectionMultiplexer>(redis);
        services.AddSingleton<ICacheService, RedisCacheService>();
        return services;
    }

    /// <summary>
    /// Adds caching services to the container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The configuration.</param>
    /// <returns>Returns the configured service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the configuration is missing.</exception>
    /// <exception cref="ArgumentException">Thrown when the preferred cache is unsupported.</exception>
    public static IServiceCollection AddCache(this IServiceCollection services, IConfiguration config)
    {
        if (IsTestEnvironment(config))
        {
            // In test environment, we can use a mock or a test container for Redis
            return services;
        }

        var cacheType = config["Cache:PreferredCache"];

        if (string.IsNullOrEmpty(cacheType))
        {
            throw new ArgumentNullException("Cache:PreferredCache configuration is missing");
        }

        if (cacheType == "Redis")
        {
            services.AddRedisCache(config);
        }
        else if (cacheType == "AzureCacheForRedis")
        {
            services.AddAzureCacheForRedis(config);
        }
        else
        {
            throw new ArgumentException($"Unsupported cache type: {cacheType}");
        }

        return services;
    }

    /// <summary>
    /// Adds infrastructure services to the container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The configuration.</param>
    /// <returns>Returns the configured service collection.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        // Register messaging services
        services.AddMessageBroker(config);

        // Register caching services
        services.AddCache(config);

        // Register other infrastructure services
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOutboxService, OutboxService>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }

}