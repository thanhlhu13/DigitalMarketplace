using System.ComponentModel.DataAnnotations;

namespace DigitalMarketplace.Application.DTOs.Orders;

public class UpdateOrderStatusRequest
{
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = null!;
}