using DigitalMarketplace.Application.DTOs.Wallets;

namespace DigitalMarketplace.Application.Interfaces;

public interface IWalletService
{
    Task<WalletResponse> GetMyWalletAsync(long userId);

    Task<List<WalletTransactionResponse>> GetMyTransactionsAsync(
        long userId);
}