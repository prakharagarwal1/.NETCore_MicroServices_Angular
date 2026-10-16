using Microsoft.EntityFrameworkCore;

namespace LearningShop.ProductService.Data;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed initial data
        Console.WriteLine("Seeding initial product data");
        Console.WriteLine("Seed complete");
    }
}