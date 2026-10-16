using System;

namespace LearningShop.MessageBus.Models;

/// <summary>
/// Base contract for all integration events published through RabbitMQ.
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}

/// <summary>
/// Default implementation shared by every integration event.
/// </summary>
public abstract class IntegrationEvent : IIntegrationEvent
{
    protected IntegrationEvent()
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
    }

    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
}