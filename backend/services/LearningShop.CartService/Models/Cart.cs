using System.ComponentModel.DataAnnotations;

namespace LearningShop.CartService.Models;

public class Cart
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<CartItem> Items { get; set; } = new();
}

public class CartItem
{
    [Key]
    public int Id { get; set; }

    public int CartId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    [StringLength(100)]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [Range(typeof(decimal), "0.01", "1000000000")]
    public decimal UnitPrice { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public decimal LineTotal => UnitPrice * Quantity;
}

/// <summary>
/// Response model for cart operations.
/// </summary>
public class CartResponse
{
    public int CartId { get; set; }
    public List<CartItemResponse> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.LineTotal);
    public int ItemCount => Items.Sum(i => i.Quantity);
}

public class CartItemResponse
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal LineTotal { get; set; }
}

/// <summary>
/// Request model for adding an item to the cart.
/// </summary>
public class AddToCartRequest
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [StringLength(100)]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [Range(typeof(decimal), "0.01", "1000000000")]
    public decimal UnitPrice { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    [StringLength(50)]
    public string Category { get; set; } = string.Empty;
}

/// <summary>
/// Request model for updating cart item quantity.
/// </summary>
public class UpdateCartItemRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

/// <summary>
/// Request model for checkout.
/// </summary>
public class CheckoutRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string CustomerName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? ShippingAddress { get; set; }
}