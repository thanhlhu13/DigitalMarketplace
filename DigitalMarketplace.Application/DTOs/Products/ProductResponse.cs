namespace DigitalMarketplace.Application.DTOs.Products;

public class ProductResponse
{
    public long Id { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}