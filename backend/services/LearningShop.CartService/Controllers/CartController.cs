using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearningShop.CartService.Data;
using LearningShop.CartService.Models;

namespace LearningShop.CartService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly CartDbContext _db;
    private readonly ILogger<CartController> _logger;

    public CartController(CartDbContext db, ILogger<CartController> logger)
    {
        _db = db;
        _logger = logger;
    }

    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? "anonymous";

    [HttpGet]
    [EndpointName("GetCart")]
    public async Task<IActionResult> GetCart()
    {
        var cart = await GetOrCreateCart();
        return Ok(MapToResponse(cart));
    }

    [HttpPost]
    [EndpointName("AddToCart")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var cart = await GetOrCreateCart();

        var existing = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (existing != null)
        {
            existing.Quantity += request.Quantity;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = request.ProductId,
                ProductName = request.ProductName,
                UnitPrice = request.UnitPrice,
                Quantity = request.Quantity,
                Category = request.Category ?? string.Empty,
                CartId = cart.Id
            });
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Added product {ProductId} (qty={Quantity}) to cart {CartId}", request.ProductId, request.Quantity, cart.Id);

        return Ok(MapToResponse(cart));
    }

    [HttpPut("{productId:int}")]
    [EndpointName("UpdateCartItem")]
    public async Task<IActionResult> UpdateCartItem(int productId, [FromBody] UpdateCartItemRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var cart = await GetOrCreateCart();
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (item == null)
            return NotFound(new { error = "Product " + productId + " not in cart." });

        item.Quantity = request.Quantity;
        item.UpdatedAt = DateTime.UtcNow;
        cart.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(MapToResponse(cart));
    }

    [HttpDelete("{productId:int}")]
    [EndpointName("RemoveFromCart")]
    public async Task<IActionResult> RemoveFromCart(int productId)
    {
        var cart = await GetOrCreateCart();
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (item == null)
            return NotFound(new { error = "Product " + productId + " not in cart." });

        cart.Items.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Removed product {ProductId} from cart {CartId}", productId, cart.Id);

        return Ok(MapToResponse(cart));
    }

    [HttpDelete]
    [EndpointName("ClearCart")]
    public async Task<IActionResult> ClearCart()
    {
        var cart = await GetOrCreateCart();
        cart.Items.Clear();
        cart.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(MapToResponse(cart));
    }

    private async Task<Cart> GetOrCreateCart()
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == UserId);

        if (cart == null)
        {
            cart = new Cart { UserId = UserId };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();
        }

        return cart;
    }

    private static CartResponse MapToResponse(Cart cart)
    {
        return new CartResponse
        {
            CartId = cart.Id,
            Items = cart.Items.Select(i => new CartItemResponse
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                Category = i.Category,
                LineTotal = i.LineTotal
            }).ToList()
        };
    }
}
