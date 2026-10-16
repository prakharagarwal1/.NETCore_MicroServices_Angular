using LearningShop.MessageBus.Models;

namespace LearningShop.MessageBus.Events;

/// <summary>
/// Published by OrderService when a new order is placed.
/// Consumers: ProductService (decrement stock), CartService (clear cart), notification services.
/// </summary>
public class OrderCreatedEvent : IntegrationEvent
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<OrderItemMessage> Items { get; set; } = new();
}

public class OrderItemMessage
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}