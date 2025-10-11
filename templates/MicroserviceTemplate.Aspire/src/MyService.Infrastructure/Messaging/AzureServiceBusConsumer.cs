using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using MyService.Domain.Common.Interfaces;

namespace MyService.Infrastructure.Messaging;

public class AzureServiceBusConsumer : IMessageConsumer
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusProcessor _processor;

    public AzureServiceBusConsumer(IConfiguration config)
    {
        var connectionString = config["MessageBroker:AzureServiceBus:ConnectionString"] ?? throw new ArgumentNullException("AzureServiceBus:ConnectionString configuration is missing");
        var queueName = config["MessageBroker:AzureServiceBus:QueueName"] ?? throw new ArgumentNullException("AzureServiceBus:QueueName configuration is missing");

        _client = new ServiceBusClient(connectionString);
        _processor = _client.CreateProcessor(queueName);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _processor.ProcessMessageAsync += async args =>
        {
            var body = args.Message.Body.ToString();

            // process...
            // Example: Deserialize and handle the message
            // var message = JsonSerializer.Deserialize<MyMessageType>(body);

            // Acknowledge the message
            // This will remove the message from the queue
            await args.CompleteMessageAsync(args.Message);
        };

        _processor.ProcessErrorAsync += args =>
        {
            // error logging
            return Task.CompletedTask;
        };

        await _processor.StartProcessingAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _processor.StopProcessingAsync(cancellationToken);
        await _processor.DisposeAsync();
    }
}
