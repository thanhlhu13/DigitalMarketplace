namespace DigitalMarketplace.Application.DTOs.Orders;

public class OrderResponse
{
    public long Id { get; set; }

    public string OrderCode { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = null!;

    public List<OrderItemResponse> Items { get; set; } = new();

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}