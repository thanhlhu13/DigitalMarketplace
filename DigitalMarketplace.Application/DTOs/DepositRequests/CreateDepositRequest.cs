using System.ComponentModel.DataAnnotations;

namespace DigitalMarketplace.Application.DTOs.DepositRequests;

public class CreateDepositRequest
{
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = null!;

    [MaxLength(100)]
    public string? TransactionCode { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}