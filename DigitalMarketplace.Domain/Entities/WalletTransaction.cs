namespace DigitalMarketplace.Domain.Entities;

public class WalletTransaction
{
    public long Id { get; set; }

    public long WalletId { get; set; }

    public long UserId { get; set; }

    public string Type { get; set; } = null!;

    public decimal Amount { get; set; }

    public decimal BalanceBefore { get; set; }

    public decimal BalanceAfter { get; set; }

    public string? ReferenceType { get; set; }

    public long? ReferenceId { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
}