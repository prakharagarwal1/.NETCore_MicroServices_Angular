using System.Text.Json;

namespace LearningShop.OrderService.Services;

/// <summary>
/// Lightweight HTTP client for checking and reserving product stock
/// via the ProductService API.
/// </summary>
public interface IStockService
{
    Task<StockCheckResult> CheckAndReserveAsync(int productId, int quantity, CancellationToken ct = default);
}

public class StockService : IStockService
{
    private readonly HttpClient _http;
    private readonly ILogger<StockService> _logger;
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public StockService(IHttpClientFactory factory, ILogger<StockService> logger)
    {
        _http = factory.CreateClient("ProductService");
        _logger = logger;
    }

    public async Task<StockCheckResult> CheckAndReserveAsync(int productId, int quantity, CancellationToken ct = default)
    {
        try
        {
            // Step 1: Get current product
            var resp = await _http.GetAsync($"/api/products/{productId}", ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Stock check failed for product {ProductId}: HTTP {Status}",
                    productId, resp.StatusCode);
                return new StockCheckResult(false, 0, "Product not found.");
            }

            var product = await resp.Content.ReadFromJsonAsync<StockProduct>(_json, ct);
            if (product == null)
                return new StockCheckResult(false, 0, "Product not found.");

            if (product.Stock < quantity)
                return new StockCheckResult(false, product.Stock,
                    $"Insufficient stock. Available: {product.Stock}, requested: {quantity}.");

            // Step 2: Decrement stock via PUT
            var content = new StringContent(
                JsonSerializer.Serialize(new { Stock = product.Stock - quantity }),
                System.Text.Encoding.UTF8, "application/json");

            var putResp = await _http.PutAsync(
                $"/api/products/{productId}", content, ct);

            if (!putResp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Stock reservation failed for product {ProductId}: HTTP {Status}",
                    productId, putResp.StatusCode);
                return new StockCheckResult(false, product.Stock, "Failed to reserve stock.");
            }

            _logger.LogInformation("Reserved {Quantity} units of product {ProductId}. Remaining: {Remaining}",
                quantity, productId, product.Stock - quantity);

            return new StockCheckResult(true, product.Stock - quantity, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stock check exception for product {ProductId}", productId);
            return new StockCheckResult(false, 0, "Stock service unavailable.");
        }
    }
}

public record StockCheckResult(bool Success, int RemainingStock, string? Error);

/// <summary>
/// Minimal projection of Product for stock operations.
/// </summary>
public class StockProduct
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}