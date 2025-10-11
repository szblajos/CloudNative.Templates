using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using MyService.Domain.Common.Interfaces;
using RabbitMQ.Client;

namespace MyService.Infrastructure.Common.Messaging;

public class RabbitMqPublisher : IMessagePublisher
{
    private readonly ConnectionFactory _factory;
    private readonly string _queueName;
    private readonly string _exchangeName;

    public RabbitMqPublisher(IConfiguration config)
    {
        _factory = new ConnectionFactory()
        {
            HostName = config["MessageBroker:RabbitMQ:Host"] ?? throw new ArgumentNullException("RabbitMQ:Host configuration is missing"),
            UserName = config["MessageBroker:RabbitMQ:Username"] ?? throw new ArgumentNullException("RabbitMQ:Username configuration is missing"),
            Password = config["MessageBroker:RabbitMQ:Password"] ?? throw new ArgumentNullException("RabbitMQ:Password configuration is missing")
        };

        _queueName = config["MessageBroker:RabbitMQ:QueueName"] ?? throw new ArgumentNullException("RabbitMQ:QueueName configuration is missing");
        _exchangeName = config["MessageBroker:RabbitMQ:ExchangeName"] ?? throw new ArgumentNullException("RabbitMQ:ExchangeName configuration is missing");
    }

    public async Task PublishAsync<T>(string type, T message, CancellationToken cancellationToken = default) where T : class
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message), "Message cannot be null");
        }

        // Ensure the connection and channel are created asynchronously
        await using var connection = await _factory.CreateConnectionAsync(cancellationToken: cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        // Declare the exchange before publishing
        await channel.ExchangeDeclareAsync(exchange: _exchangeName, type: "direct", durable: true, autoDelete: false, cancellationToken: cancellationToken);
        // Create a queue if it doesn't already exist
        await channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        // Bind the queue to the exchange
        await channel.QueueBindAsync(queue: _queueName, exchange: _exchangeName, routingKey: _queueName, cancellationToken: cancellationToken);

        // Serialize the message to JSON and publish it
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        await channel.BasicPublishAsync(exchange: _exchangeName, routingKey: _queueName, body: body, cancellationToken: cancellationToken);
    }
}
