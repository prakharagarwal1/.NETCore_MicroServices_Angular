using LearningShop.MessageBus.Models;

namespace LearningShop.MessageBus;

/// <summary>
/// Abstraction over the message broker. Consumers depend on this interface
/// (and the DI registration in MessageBusExtensions) rather than MassTransit directly.
/// </summary>
public interface IMessageBus
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
}