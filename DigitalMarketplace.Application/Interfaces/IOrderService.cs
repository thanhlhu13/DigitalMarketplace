using DigitalMarketplace.Application.DTOs.Orders;

namespace DigitalMarketplace.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(long userId);

    Task<List<OrderResponse>> GetMyOrdersAsync(long userId);

    Task<OrderResponse> GetByIdAsync(long userId, long orderId);

    Task UpdateStatusAsync(
        long orderId,
        UpdateOrderStatusRequest request);

    Task CancelAsync(
        long userId,
        long orderId);
}