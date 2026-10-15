using Microsoft.AspNetCore.Mvc;

namespace LearningShop.ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private static readonly Product[] Products =
    [
        new(1, "Laptop", 75000),
        new(2, "Keyboard", 2500),
        new(3, "Mouse", 1200),
         new(4, "Mouse1", 1200),
               new(5, "Mouse1", 1200)
    ];

    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetProducts()
    {
        return Ok(Products);
    }
}

public record Product(int Id, string Name, decimal Price);