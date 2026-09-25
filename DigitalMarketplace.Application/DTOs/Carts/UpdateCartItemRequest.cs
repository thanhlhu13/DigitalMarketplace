using System.ComponentModel.DataAnnotations;

namespace DigitalMarketplace.Application.DTOs.Carts;

public class UpdateCartItemRequest
{
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}