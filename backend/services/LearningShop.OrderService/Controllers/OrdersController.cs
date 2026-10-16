using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningShop.MessageBus;
using LearningShop.MessageBus.Events;
using LearningShop.MessageBus.Models;

namespace LearningShop.OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private static readonly ConcurrentDictionary<int, Order> Orders = new(
        new Dictionary<int, Order>
        {
            [1001] = new(1001, "Customer A", 77500m, OrderStatus.Pending),
            [1002] = new(1002, "Customer B", 2500m, OrderStatus.Confirmed),
            [1003] = new(1003, "Customer C", 1200m, OrderStatus.Shipped),
            [1004] = new(1004, "Customer C", 1200m, OrderStatus.Shipped)
        });

    private static int _nextId = 1004;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMessageBus messageBus, ILogger<OrdersController> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    // GET: /api/orders
    [HttpGet]
    public ActionResult<IEnumerable<Order>> GetOrders()
    {
        return Ok(Orders.Values.OrderBy(o => o.Id));
    }

    // GET: /api/orders/1001
    [HttpGet("{id:int}")]
    public ActionResult<Order> GetOrder(int id)
    {
        if (!Orders.TryGetValue(id, out var order))
        {
            return NotFound();
        }
        return Ok(order);
    }

    // POST: /api/orders
    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var id = Interlocked.Increment(ref _nextId);

        var order = new Order(
            id,
            request.CustomerName.Trim(),
            request.Total,
            OrderStatus.Pending);

        Orders[id] = order;

        // Publish integration event to RabbitMQ
        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerName = order.CustomerName,
            Total = order.Total,
            Items = new List<OrderItemMessage>
            {
                new() { ProductId = 1, Quantity = 1, UnitPrice = order.Total }
            }
        };

        await _messageBus.PublishAsync(orderCreatedEvent, cancellationToken);
        _logger.LogInformation("Published OrderCreatedEvent for OrderId={OrderId}", order.Id);

        return CreatedAtAction(
            nameof(GetOrder),
            new { id = order.Id },
            order);
    }

    // PUT: /api/orders/1001/status
    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Admin,Seller")]
    public ActionResult<Order> UpdateOrderStatus(
        int id,
        [FromBody] UpdateOrderStatusRequest request)
    {
        if (!Enum.IsDefined<OrderStatus>(request.Status))
        {
            return BadRequest(new { error = $"Invalid order status: {request.Status}" });
        }

        if (!Orders.TryGetValue(id, out var order))
        {
            return NotFound(new { error = $"Order with ID {id} not found." });
        }

        var newStatus = (OrderStatus)request.Status;

        var allowedTransitions = new Dictionary<OrderStatus, HashSet<OrderStatus>>
        {
            [OrderStatus.Pending] = [OrderStatus.Confirmed, OrderStatus.Cancelled],
            [OrderStatus.Confirmed] = [OrderStatus.Shipped, OrderStatus.Cancelled],
            [OrderStatus.Shipped] = [OrderStatus.Delivered],
            [OrderStatus.Delivered] = [],
            [OrderStatus.Cancelled] = []
        };

        if (!allowedTransitions[order.Status].Contains(newStatus))
        {
            return BadRequest(new
            {
                error = $"Cannot transition from '{order.Status}' to '{newStatus}'.",
                allowedTransitions = allowedTransitions[order.Status].Select(s => s.ToString())
            });
        }

        var updatedOrder = order with { Status = newStatus };
        Orders[id] = updatedOrder;

        return Ok(updatedOrder);
    }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}

public record Order(
    int Id,
    string CustomerName,
    decimal Total,
    OrderStatus Status);

public class CreateOrderRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string CustomerName { get; set; } = string.Empty;

    [Range(typeof(decimal), "1", "1000000000")]
    public decimal Total { get; set; }
}

public class UpdateOrderStatusRequest
{
    [Required]
    public OrderStatus Status { get; set; }
}