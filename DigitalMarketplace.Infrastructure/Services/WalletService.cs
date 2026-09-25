using DigitalMarketplace.Application.DTOs.Wallets;
using DigitalMarketplace.Application.Interfaces;
using DigitalMarketplace.Domain.Entities;
using DigitalMarketplace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalMarketplace.Infrastructure.Services;

public class WalletService : IWalletService
{
    private readonly ApplicationDbContext _context;

    public WalletService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WalletResponse> GetMyWalletAsync(long userId)
    {
        var wallet = await GetOrCreateWalletAsync(userId);

        return new WalletResponse
        {
            Id = wallet.Id,
            Balance = wallet.Balance,
            UpdatedAt = wallet.UpdatedAt
        };
    }

    public async Task<List<WalletTransactionResponse>> GetMyTransactionsAsync(
        long userId)
    {
        var wallet = await GetOrCreateWalletAsync(userId);

        return await _context.WalletTransactions
            .AsNoTracking()
            .Where(x =>
                x.WalletId == wallet.Id &&
                x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new WalletTransactionResponse
            {
                Id = x.Id,
                Type = x.Type,
                Amount = x.Amount,
                BalanceBefore = x.BalanceBefore,
                BalanceAfter = x.BalanceAfter,
                ReferenceType = x.ReferenceType,
                ReferenceId = x.ReferenceId,
                Description = x.Description,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    private async Task<Wallet> GetOrCreateWalletAsync(long userId)
    {
        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (wallet != null)
            return wallet;

        wallet = new Wallet
        {
            UserId = userId,
            Balance = 0,
            UpdatedAt = null
        };

        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync();

        return wallet;
    }
}