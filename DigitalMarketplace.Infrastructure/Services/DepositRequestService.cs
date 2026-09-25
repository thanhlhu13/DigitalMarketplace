using DigitalMarketplace.Application.DTOs.DepositRequests;
using DigitalMarketplace.Application.Interfaces;
using DigitalMarketplace.Domain.Entities;
using DigitalMarketplace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalMarketplace.Infrastructure.Services;

public class DepositRequestService : IDepositRequestService
{
    private readonly ApplicationDbContext _context;

    public DepositRequestService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DepositRequestResponse> CreateAsync(
        long userId,
        CreateDepositRequest request)
    {
        // Kiểm tra số tiền phải lớn hơn 0.
        if (request.Amount <= 0)
            throw new InvalidOperationException(
                "Deposit amount must be greater than zero.");

        // Kiểm tra User tồn tại và đang hoạt động.
        var userExists = await _context.Users
            .AnyAsync(x => x.Id == userId && x.IsActive);

        if (!userExists)
            throw new KeyNotFoundException(
                "User not found or inactive.");

        var depositRequest = new DepositRequest
        {
            UserId = userId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            TransactionCode = request.TransactionCode,
            Status = "Pending",
            Note = request.Note,
            CreatedAt = DateTime.UtcNow
        };

        _context.DepositRequests.Add(depositRequest);

        await _context.SaveChangesAsync();

        return MapToResponse(depositRequest);
    }

    public async Task<List<DepositRequestResponse>> GetMyRequestsAsync(
        long userId)
    {
        return await _context.DepositRequests
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new DepositRequestResponse
            {
                Id = x.Id,
                Amount = x.Amount,
                PaymentMethod = x.PaymentMethod,
                TransactionCode = x.TransactionCode,
                Status = x.Status,
                Note = x.Note,
                ProcessedBy = x.ProcessedBy,
                ProcessedAt = x.ProcessedAt,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<DepositRequestResponse> GetByIdAsync(
        long userId,
        long requestId)
    {
        var depositRequest = await _context.DepositRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Id == requestId &&
                x.UserId == userId);

        if (depositRequest == null)
            throw new KeyNotFoundException(
                "Deposit request not found.");

        return MapToResponse(depositRequest);
    }

    public async Task<DepositRequestResponse> UpdateStatusAsync(
        long adminUserId,
        long requestId,
        UpdateDepositRequestStatus request)
    {
        // Chỉ cho phép hai trạng thái mà Admin có thể xử lý.
        if (request.Status != "Approved" &&
            request.Status != "Rejected")
        {
            throw new InvalidOperationException(
                "Status must be Approved or Rejected.");
        }

        // Transaction đảm bảo DepositRequest và Wallet
        // được cập nhật đồng bộ.
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var depositRequest = await _context.DepositRequests
                .FirstOrDefaultAsync(x => x.Id == requestId);

            if (depositRequest == null)
                throw new KeyNotFoundException(
                    "Deposit request not found.");

            // Chỉ xử lý request đang Pending.
            if (depositRequest.Status != "Pending")
                throw new InvalidOperationException(
                    "Only pending deposit requests can be processed.");

            var adminExists = await _context.Users
                .AnyAsync(x => x.Id == adminUserId && x.IsActive);

            if (!adminExists)
                throw new UnauthorizedAccessException(
                    "Admin user not found or inactive.");

            // Cập nhật thông tin xử lý request.
            depositRequest.Status = request.Status;
            depositRequest.ProcessedBy = adminUserId;
            depositRequest.ProcessedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(request.Note))
                depositRequest.Note = request.Note;

            // Nếu Rejected thì không thay đổi Wallet.
            if (request.Status == "Rejected")
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return MapToResponse(depositRequest);
            }

            // Approved -> lấy Wallet của User.
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(x =>
                    x.UserId == depositRequest.UserId);

            // Nếu User chưa có Wallet thì tạo mới.
            if (wallet == null)
            {
                wallet = new Wallet
                {
                    UserId = depositRequest.UserId,
                    Balance = 0,
                    UpdatedAt = null
                };

                _context.Wallets.Add(wallet);

                await _context.SaveChangesAsync();
            }

            var balanceBefore = wallet.Balance;
            var balanceAfter =
                balanceBefore + depositRequest.Amount;

            // Cộng tiền vào Wallet.
            wallet.Balance = balanceAfter;
            wallet.UpdatedAt = DateTime.UtcNow;

            // Ghi lịch sử giao dịch.
            var walletTransaction = new WalletTransaction
            {
                WalletId = wallet.Id,
                UserId = depositRequest.UserId,
                Type = "Deposit",
                Amount = depositRequest.Amount,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                ReferenceType = "DepositRequest",
                ReferenceId = depositRequest.Id,
                Description =
                    $"Deposit approved: {depositRequest.PaymentMethod}",
                CreatedAt = DateTime.UtcNow
            };

            _context.WalletTransactions.Add(walletTransaction);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return MapToResponse(depositRequest);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static DepositRequestResponse MapToResponse(
        DepositRequest depositRequest)
    {
        return new DepositRequestResponse
        {
            Id = depositRequest.Id,
            Amount = depositRequest.Amount,
            PaymentMethod = depositRequest.PaymentMethod,
            TransactionCode = depositRequest.TransactionCode,
            Status = depositRequest.Status,
            Note = depositRequest.Note,
            ProcessedBy = depositRequest.ProcessedBy,
            ProcessedAt = depositRequest.ProcessedAt,
            CreatedAt = depositRequest.CreatedAt
        };
    }
}