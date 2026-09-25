namespace DigitalMarketplace.Application.DTOs.Wallets;

public class WalletResponse
{
    public long Id { get; set; }

    public decimal Balance { get; set; }

    public DateTime? UpdatedAt { get; set; }
}