namespace DigitalMarketplace.Application.DTOs.Carts;

public class CartResponse
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public decimal TotalAmount { get; set; }

    public List<CartItemResponse> Items { get; set; } = new();

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}