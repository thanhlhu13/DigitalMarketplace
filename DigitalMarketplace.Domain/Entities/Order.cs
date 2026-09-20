namespace DigitalMarketplace.Domain.Entities;

public class Order
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string OrderCode { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}