using MassTransit;

namespace LearningShop.MessageBus.Services;

/// <summary>
/// Default <see cref="IMessageBus"/> implementation backed by MassTransit + RabbitMQ.
/// </summary>
public class MessageBus : IMessageBus
{
    private readonly IBus _bus;

    public MessageBus(IBus bus)
    {
        _bus = bus;
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        await _bus.Publish(message, cancellationToken);
    }
}