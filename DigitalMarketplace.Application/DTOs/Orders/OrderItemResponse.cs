namespace DigitalMarketplace.Application.DTOs.Orders;

public class OrderItemResponse
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }
}