using System.ComponentModel.DataAnnotations;

namespace DigitalMarketplace.Application.DTOs.DepositRequests;

public class UpdateDepositRequestStatus
{
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = null!;

    [MaxLength(500)]
    public string? Note { get; set; }
}