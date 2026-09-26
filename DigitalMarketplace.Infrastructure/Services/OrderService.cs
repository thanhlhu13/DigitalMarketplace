using DigitalMarketplace.Application.DTOs.Orders;
using DigitalMarketplace.Application.Interfaces;
using DigitalMarketplace.Domain.Entities;
using DigitalMarketplace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalMarketplace.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates an order from the current user's cart.
    /// The whole checkout process is executed inside a database transaction
    /// to keep Order, OrderItems, Stock and CartItems consistent.
    /// </summary>
    public async Task<OrderResponse> CreateAsync(long userId)
    {
        // Lấy Cart của user.
        var cart = await _context.Carts
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (cart == null)
            throw new InvalidOperationException(
                "Cart is empty.");

        // Lấy các sản phẩm trong Cart.
        var cartItems = await _context.CartItems
            .Where(x => x.CartId == cart.Id)
            .ToListAsync();

        if (cartItems.Count == 0)
            throw new InvalidOperationException(
                "Cart is empty.");

        // Lấy Product tương ứng.
        var productIds = cartItems
            .Select(x => x.ProductId)
            .Distinct()
            .ToList();

        var products = await _context.Products
            .Where(x => productIds.Contains(x.Id))
            .ToListAsync();

        // Kiểm tra sản phẩm và tồn kho trước khi bắt đầu transaction.
        foreach (var cartItem in cartItems)
        {
            var product = products.FirstOrDefault(
                x => x.Id == cartItem.ProductId);

            if (product == null)
                throw new KeyNotFoundException(
                    $"Product {cartItem.ProductId} not found.");

            if (!product.IsActive)
                throw new InvalidOperationException(
                    $"Product '{product.Name}' is inactive.");

            if (product.StockQuantity < cartItem.Quantity)
                throw new InvalidOperationException(
                    $"Product '{product.Name}' does not have enough stock.");
        }

        // Tính tổng tiền từ giá hiện tại của Product.
        var totalAmount = cartItems.Sum(cartItem =>
        {
            var product = products.First(
                x => x.Id == cartItem.ProductId);

            return product.Price * cartItem.Quantity;
        });

        if (totalAmount <= 0)
            throw new InvalidOperationException(
                "Order total must be greater than zero.");

        // Bắt đầu transaction.
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // Lấy Wallet của User.
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (wallet == null)
                throw new InvalidOperationException(
                    "Wallet not found.");

            // Kiểm tra số dư.
            if (wallet.Balance < totalAmount)
                throw new InvalidOperationException(
                    "Insufficient wallet balance.");

            var balanceBefore = wallet.Balance;
            var balanceAfter = balanceBefore - totalAmount;

            // Trừ tiền Wallet.
            wallet.Balance = balanceAfter;
            wallet.UpdatedAt = DateTime.UtcNow;

            // Tạo Order.
            var order = new Order
            {
                UserId = userId,
                OrderCode =
                    $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"
                        .ToUpperInvariant(),
                TotalAmount = totalAmount,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            // Tạo OrderItems và trừ Stock.
            foreach (var cartItem in cartItems)
            {
                var product = products.First(
                    x => x.Id == cartItem.ProductId);

                var itemTotal =
                    product.Price * cartItem.Quantity;

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = cartItem.Quantity,
                    TotalPrice = itemTotal
                };

                _context.OrderItems.Add(orderItem);

                // Trừ tồn kho.
                product.StockQuantity -= cartItem.Quantity;
            }

            // Ghi lịch sử thanh toán.
            var walletTransaction = new WalletTransaction
            {
                WalletId = wallet.Id,
                UserId = userId,
                Type = "Payment",
                Amount = totalAmount,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                ReferenceType = "Order",
                ReferenceId = order.Id,
                Description = $"Payment for order {order.OrderCode}",
                CreatedAt = DateTime.UtcNow
            };

            _context.WalletTransactions.Add(walletTransaction);

            // Xóa CartItems sau khi tạo Order thành công.
            _context.CartItems.RemoveRange(cartItems);

            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return await BuildOrderResponseAsync(order.Id, userId);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Retrieves all orders belonging to the current user.
    /// </summary>
    public async Task<List<OrderResponse>> GetMyOrdersAsync(
        long userId)
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var responses = new List<OrderResponse>();

        foreach (var order in orders)
        {
            responses.Add(
                await BuildOrderResponseAsync(
                    order.Id,
                    userId));
        }

        return responses;
    }

    /// <summary>
    /// Retrieves one order belonging to the current user.
    /// </summary>
    public async Task<OrderResponse> GetByIdAsync(
        long userId,
        long orderId)
    {
        var orderExists = await _context.Orders
            .AnyAsync(x =>
                x.Id == orderId &&
                x.UserId == userId);

        if (!orderExists)
        {
            throw new KeyNotFoundException(
                "Order does not exist.");
        }

        return await BuildOrderResponseAsync(
            orderId,
            userId);
    }

    /// <summary>
    /// Updates an order status.
    /// This operation is intended for Admin users.
    /// </summary>
    public async Task UpdateStatusAsync(
        long orderId,
        UpdateOrderStatusRequest request)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(x => x.Id == orderId);

        if (order == null)
        {
            throw new KeyNotFoundException(
                "Order does not exist.");
        }
        var allowedTransitions = new Dictionary<string, string[]>
        {
            ["Pending"] = ["Confirmed", "Cancelled"],
            ["Confirmed"] = ["Processing", "Cancelled"],
            ["Processing"] = ["Completed"],
            ["Completed"] = [],
            ["Cancelled"] = []
        };

        if (!allowedTransitions.TryGetValue(order.Status, out var allowedStatuses) ||
            !allowedStatuses.Contains(request.Status))
        {
            throw new InvalidOperationException(
                $"Order cannot be changed from '{order.Status}' to '{request.Status}'.");
        }

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Builds the response for an order and its items.
    /// </summary>
    private async Task<OrderResponse> BuildOrderResponseAsync(
        long orderId,
        long userId)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Id == orderId &&
                x.UserId == userId);

        if (order == null)
        {
            throw new KeyNotFoundException(
                "Order does not exist.");
        }

        var items = await _context.OrderItems
            .AsNoTracking()
            .Where(x => x.OrderId == orderId)
            .Select(x => new OrderItemResponse
            {
                Id = x.Id,
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                UnitPrice = x.UnitPrice,
                Quantity = x.Quantity,
                TotalPrice = x.TotalPrice ?? 0
            })
            .ToListAsync();

        return new OrderResponse
        {
            Id = order.Id,
            OrderCode = order.OrderCode,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            Items = items,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }

    /// <summary>
    /// Generates a human-readable order code.
    /// </summary>
    private static string GenerateOrderCode()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"
            .ToUpperInvariant();
    }

    public async Task CancelAsync(
    long userId,
    long orderId)
    {
        // Start a database transaction to keep wallet,
        // stock, and order changes consistent.
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // Find the order belonging to the current user.
            var order = await _context.Orders
                .FirstOrDefaultAsync(x =>
                    x.Id == orderId &&
                    x.UserId == userId);

            if (order == null)
            {
                throw new KeyNotFoundException(
                    "Order does not exist.");
            }

            // Only pending orders can be cancelled.
            if (order.Status != "Pending")
            {
                throw new InvalidOperationException(
                    "Only pending orders can be cancelled.");
            }

            // Get all items belonging to the order.
            var orderItems = await _context.OrderItems
                .Where(x => x.OrderId == order.Id)
                .ToListAsync();

            // Get the user's wallet.
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (wallet == null)
            {
                throw new InvalidOperationException(
                    "Wallet not found.");
            }

            var balanceBefore = wallet.Balance;
            var balanceAfter =
                balanceBefore + order.TotalAmount;

            // Refund the order amount to the user's wallet.
            wallet.Balance = balanceAfter;
            wallet.UpdatedAt = DateTime.UtcNow;

            // Restore product stock.
            foreach (var orderItem in orderItems)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(x =>
                        x.Id == orderItem.ProductId);

                if (product != null)
                {
                    product.StockQuantity += orderItem.Quantity;
                }
            }

            // Update the order status.
            order.Status = "Cancelled";
            order.UpdatedAt = DateTime.UtcNow;

            // Create a wallet transaction for the refund.
            var walletTransaction = new WalletTransaction
            {
                WalletId = wallet.Id,
                UserId = userId,
                Type = "Refund",
                Amount = order.TotalAmount,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                ReferenceType = "Order",
                ReferenceId = order.Id,
                Description = $"Refund for order {order.OrderCode}",
                CreatedAt = DateTime.UtcNow
            };

            _context.WalletTransactions.Add(walletTransaction);

            await _context.SaveChangesAsync();

            // Commit all changes together.
            await transaction.CommitAsync();
        }
        catch
        {
            // Roll back all changes if any operation fails.
            await transaction.RollbackAsync();
            throw;
        }
    }
}