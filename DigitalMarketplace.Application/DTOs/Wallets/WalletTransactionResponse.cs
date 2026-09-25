namespace DigitalMarketplace.Application.DTOs.Wallets;

public class WalletTransactionResponse
{
    public long Id { get; set; }

    public string Type { get; set; } = null!;

    public decimal Amount { get; set; }

    public decimal BalanceBefore { get; set; }

    public decimal BalanceAfter { get; set; }

    public string? ReferenceType { get; set; }

    public long? ReferenceId { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
}