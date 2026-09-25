namespace DigitalMarketplace.Application.DTOs.Carts;

public class CartItemResponse
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string ProductSlug { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }
}