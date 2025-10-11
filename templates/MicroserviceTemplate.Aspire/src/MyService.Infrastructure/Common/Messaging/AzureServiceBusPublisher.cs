using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using MyService.Domain.Common.Interfaces;
using System.Text.Json;

namespace MyService.Infrastructure.Common.Messaging;

public class AzureServiceBusPublisher : IMessagePublisher
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;

    public AzureServiceBusPublisher(IConfiguration config)
    {
        var connectionString = config["MessageBroker:AzureServiceBus:ConnectionString"] ?? throw new ArgumentNullException("AzureServiceBus:ConnectionString configuration is missing");
        var queueName = config["MessageBroker:AzureServiceBus:QueueName"] ?? throw new ArgumentNullException("AzureServiceBus:QueueName configuration is missing");

        _client = new ServiceBusClient(connectionString);
        _sender = _client.CreateSender(queueName);
    }

    public async Task PublishAsync<T>(string type, T message, CancellationToken cancellationToken = default) where T : class
    {
        var json = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(json)
        {
            ApplicationProperties = { ["MessageType"] = type }
        };

        await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }
}
