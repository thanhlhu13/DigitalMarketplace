namespace DigitalMarketplace.Domain.Entities;

public class Wallet
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public decimal Balance { get; set; }

    public DateTime? UpdatedAt { get; set; }
}