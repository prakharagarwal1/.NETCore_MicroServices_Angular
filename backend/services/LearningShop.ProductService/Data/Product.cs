using System.ComponentModel.DataAnnotations;

namespace LearningShop.ProductService.Data;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(typeof(decimal), "0.01", "1000000000")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
}