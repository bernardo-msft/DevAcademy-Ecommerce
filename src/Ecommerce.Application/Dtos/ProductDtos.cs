using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Application.Dtos;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class CreateProductDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    [Range(0.01, 100000)]
    public decimal Price { get; set; }
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
    [Required]
    public Guid CategoryId { get; set; }
    public IFormFile? ImageFile { get; set; }
}

public class UpdateProductDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    [Range(0.01, 100000)]
    public decimal Price { get; set; }
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
    [Required]
    public Guid CategoryId { get; set; }
    public IFormFile? ImageFile { get; set; }
}