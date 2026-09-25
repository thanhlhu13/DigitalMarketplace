namespace DigitalMarketplace.Application.DTOs.DepositRequests;

public class DepositRequestResponse
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string? TransactionCode { get; set; }
    public string Status { get; set; } = null!;
    public string? Note { get; set; }
    public long? ProcessedBy { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}