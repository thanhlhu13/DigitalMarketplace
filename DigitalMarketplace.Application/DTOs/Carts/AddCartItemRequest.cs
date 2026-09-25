using System.ComponentModel.DataAnnotations;

namespace DigitalMarketplace.Application.DTOs.Carts;

public class AddCartItemRequest
{
    [Required]
    public long ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}