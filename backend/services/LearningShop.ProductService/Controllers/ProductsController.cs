using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearningShop.ProductService.Data;

namespace LearningShop.ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly ProductDbContext _db;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ProductDbContext db, ILogger<ProductsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    [EndpointName("GetProducts")]
    [EndpointSummary("List products")]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? category = null,
        [FromQuery] bool activeOnly = true)
    {
        var query = _db.Products.AsQueryable();

        if (activeOnly)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => EF.Functions.Like(p.Category, $"%{category}%"));

        var products = await query
            .OrderBy(p => p.Id)
            .Select(p => new ProductSummary(p))
            .ToArrayAsync();

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    [EndpointName("GetProduct")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _db.Products
            .Select(p => new ProductSummary(p))
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound(new { error = $"Product with ID {id} not found." });

        return Ok(product);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Price = request.Price,
            Stock = request.Stock,
            Category = request.Category?.Trim() ?? string.Empty,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Product created: {ProductName} (ID={ProductId})", product.Name, product.Id);

        var summary = new ProductSummary(product);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, summary);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
            return NotFound(new { error = $"Product with ID {id} not found." });

        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim() ?? string.Empty;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.Category = request.Category?.Trim() ?? string.Empty;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Product updated: {ProductName} (ID={ProductId})", product.Name, product.Id);

        return Ok(new ProductSummary(product));
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
            return NotFound(new { error = $"Product with ID {id} not found." });

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Product deleted: {ProductName} (ID={ProductId})", product.Name, product.Id);

        return NoContent();
    }
}

public record ProductSummary
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }
    public string Category { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    public ProductSummary() { }
    public ProductSummary(Product p)
    {
        Id = p.Id;
        Name = p.Name;
        Description = p.Description;
        Price = p.Price;
        Stock = p.Stock;
        Category = p.Category;
        IsActive = p.IsActive;
        CreatedAt = p.CreatedAt;
        UpdatedAt = p.UpdatedAt;
    }
}

public class CreateProductRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [Range(typeof(decimal), "0.01", "1000000000")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; } = 0;

    [StringLength(50)]
    public string? Category { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdateProductRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [Range(typeof(decimal), "0.01", "1000000000")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; } = 0;

    [StringLength(50)]
    public string? Category { get; set; }

    public bool IsActive { get; set; } = true;
}



