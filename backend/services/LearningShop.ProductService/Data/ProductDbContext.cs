using Microsoft.EntityFrameworkCore;

namespace LearningShop.ProductService.Data;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbcontextOptions<ProductDbContext> options)
        : base(options)
    {
    }

    public Dbset<Product> Products { get; set; } = null!;

    protected override void OnConfiguring(DbcontextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=learningshop.db");
        }
    }

    protected override void OnCreating(ModelBuilder modelBuilder)
    {
        base.OnCreating(modelBuilder);

        var products = new List<Product>
        {
            new() { Id = 1, Name = "Laptop", Description = "High-performance laptop for development", Price = 75000, Stock = 15, Category = "Electronics", CreatedAt = new DateTime(2026, 1, 1) },
            new() { Id = 2, Name = "Keyboard", Description = "Mechanical keyboard", Price = 2500, Stock = 50, Category = "Accessories", CreatedAt = new DateTime(2026, 1, 1) },
            new() { Id = 3, Name = "Mouse", Description = "Wireless optical mouse", Price = 1200, Stock = 100, Category = "Accessories", CreatedAt = new DateTime(2026, 1, 1) },
            new() { Id = 4, Name = "Monitor", Description = "27-inch 4K monitor", Price = 35000, Stock = 20, Category = "Electronics", CreatedAt = new DateTime(2026, 1, 1) },
            new() { Id = 5, Name = "Headphones", Description = "Noise-cancelling headphones", Price = 5000, Stock = 30, Category = "Accessories", CreatedAt = new DateTime(2026, 1, 1) }
        };

        modelBuilder.Entity<Product>().HasData(products);
    }
}