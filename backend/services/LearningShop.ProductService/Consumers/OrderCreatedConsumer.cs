using MassTransit;
using Microsoft.EntityFrameworkCore;
using LearningShop.MessageBus.Events;
using LearningShop.ProductService.Data;

namespace LearningShop.ProductService.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly ProductDbContext _db;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(ProductDbContext db, ILogger<OrderCreatedConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Consuming OrderCreatedEvent: OrderId={OrderId}, Items={ItemCount}",
            message.OrderId, message.Items.Count);

        foreach (var item in message.Items)
        {
            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.Id == item.ProductId);

            if (product == null)
            {
                _logger.LogWarning(
                    "Cannot decrement stock: Product {ProductId} not found (OrderId={OrderId})",
                    item.ProductId, message.OrderId);
                continue;
            }

            product.Stock = Math.Max(0, product.Stock - item.Quantity);
            product.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }
}
