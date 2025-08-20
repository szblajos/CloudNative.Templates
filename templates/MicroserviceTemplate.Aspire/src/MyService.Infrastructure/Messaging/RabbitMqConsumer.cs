
using System.Text;
using Microsoft.Extensions.Configuration;
using MyService.Domain.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MyService.Infrastructure.Messaging;

public class RabbitMqConsumer : IMessageConsumer
{
    private IChannel _channel = null!;
    private readonly ConnectionFactory _factory;
    public readonly string _queueName;

    public RabbitMqConsumer(IConfiguration config)
    {
        _factory = new ConnectionFactory()
        {
            HostName = config["MessageBroker:RabbitMQ:Host"] ?? throw new ArgumentNullException("RabbitMQ:Host configuration is missing"),
            UserName = config["MessageBroker:RabbitMQ:Username"] ?? throw new ArgumentNullException("RabbitMQ:Username configuration is missing"),
            Password = config["MessageBroker:RabbitMQ:Password"] ?? throw new ArgumentNullException("RabbitMQ:Password configuration is missing"),
        };

        _queueName = config["MessageBroker:RabbitMQ:QueueName"] ?? throw new ArgumentNullException("RabbitMQ:QueueName configuration is missing");
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _factory.CreateConnectionAsync();
        _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            // feldolgozás...

        };

        await _channel.BasicConsumeAsync(queue: _queueName, autoAck: true, consumer: consumer, cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _channel.CloseAsync(cancellationToken: cancellationToken);
        _channel.DisposeAsync();
        _channel = null!; // Clear the channel reference
        return Task.CompletedTask;
    }
}

