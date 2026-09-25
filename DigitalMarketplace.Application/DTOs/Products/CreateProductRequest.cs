using System.ComponentModel.DataAnnotations;

namespace DigitalMarketplace.Application.DTOs.Products;

public class CreateProductRequest
{
    [Required]
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(250)]
    public string Slug { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
}