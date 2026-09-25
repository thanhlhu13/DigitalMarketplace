using System.Security.Claims;
using DigitalMarketplace.Application.DTOs.Wallets;
using DigitalMarketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalMarketplace.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    // GET: api/Wallet
    // Lấy thông tin ví của user hiện tại.
    [HttpGet]
    public async Task<ActionResult<WalletResponse>> GetMyWallet()
    {
        var userId = GetCurrentUserId();

        var wallet = await _walletService.GetMyWalletAsync(userId);

        return Ok(wallet);
    }

    // GET: api/Wallet/transactions
    // Lấy lịch sử giao dịch của user hiện tại.
    [HttpGet("transactions")]
    public async Task<ActionResult<List<WalletTransactionResponse>>> GetMyTransactions()
    {
        var userId = GetCurrentUserId();

        var transactions =
            await _walletService.GetMyTransactionsAsync(userId);

        return Ok(transactions);
    }

    private long GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdClaim == null ||
            !long.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException(
                "User is not authenticated.");
        }

        return userId;
    }
}