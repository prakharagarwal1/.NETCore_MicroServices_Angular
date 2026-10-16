using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LearningShop.MessageBus;

/// <summary>
/// Extension methods for wiring MassTransit + RabbitMQ into an <see cref="IServiceCollection"/>.
/// </summary>
public static class MessageBusExtensions
{
    /// <summary>
    /// Registers MassTransit with RabbitMQ as the transport, plus the <see cref="IMessageBus"/> facade.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration (reads <c>RabbitMQ:Host</c>, <c>RabbitMQ:Username</c>, <c>RabbitMQ:Password</c>).</param>
    /// <param name="configureConsumers">Optional callback to register consumer classes (e.g. <c>cfg.AddConsumer&lt;OrderCreatedConsumer&gt;()</c>).</param>
    public static void AddMessageBus(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IRabbitMqBusFactoryConfigurator>? configureConsumers = null)
    {
        var rabbitHost = configuration["RabbitMQ:Host"] ?? "localhost";
        var rabbitUser = configuration["RabbitMQ:Username"] ?? "guest";
        var rabbitPass = configuration["RabbitMQ:Password"] ?? "guest";

        services.AddMassTransit(bus =>
        {
            bus.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(rabbitHost, "/", h =>
                {
                    h.Username(rabbitUser);
                    h.Password(rabbitPass);
                });

                // Register any consumers supplied by the host application.
                configureConsumers?.Invoke(cfg);
            });
        });

        services.AddTransient<IMessageBus, Services.MessageBus>();
    }
}