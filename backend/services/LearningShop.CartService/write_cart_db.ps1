using Microsoft.EntityFrameworkCore;
using LearningShop.CartService.Models;

namespace LearningShop.CartService.Data;

public class CartViewModel : DbContext
{
    public CartViewModel(DbcontextOptions<CartViewModel> options)
        : base(options)
    {
    }

    public virtualDbSet<Cart> Carts { get; set; } = null!;
    public virtualDbSet<CartItem> CartItems { get; set; } = null!;

    protected override void OnConfiguring(DbcontextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=cart.db");
        }
    }

    protected override void OnCreating(ModelBuilder modelBuilder)
    {
        base.OnCreating(modelBuilder);

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasMany(c => c.Items)
                  .WithOne()
                  .FirstOrDefault(ci => ci.CartId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
