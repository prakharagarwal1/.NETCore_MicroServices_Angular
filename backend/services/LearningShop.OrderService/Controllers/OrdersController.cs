using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace LearningShop.OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private static readonly ConcurrentDictionary<int, Order> Orders = new(
        new Dictionary<int, Order>
        {
            [1001] = new(1001, "Customer A", 77500m, "Pending"),
            [1002] = new(1002, "Customer B", 2500m, "Confirmed"),
            [1003] = new(1003, "Customer C", 1200m, "Shipped"),
            [1004] = new(1004, "Customer C", 1200m, "Shipped")
        });

    private static int _nextId = 1004;

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
    public ActionResult<Order> CreateOrder(
        [FromBody] CreateOrderRequest request)
    {
        var id = Interlocked.Increment(ref _nextId);

        var order = new Order(
            id,
            request.CustomerName.Trim(),
            request.Total,
            "Pending");

        Orders[id] = order;

        return CreatedAtAction(
            nameof(GetOrder),
            new { id = order.Id },
            order);
    }
}

public record Order(
    int Id,
    string CustomerName,
    decimal Total,
    string Status);

public class CreateOrderRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string CustomerName { get; set; } = string.Empty;

    [Range(typeof(decimal), "1", "1000000000")]
    public decimal Total { get; set; }
}