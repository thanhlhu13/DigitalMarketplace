using DigitalMarketplace.Application.DTOs.DepositRequests;

namespace DigitalMarketplace.Application.Interfaces;

public interface IDepositRequestService
{
    // User tạo yêu cầu nạp tiền.
    Task<DepositRequestResponse> CreateAsync(
        long userId,
        CreateDepositRequest request);

    // User xem lịch sử các yêu cầu nạp tiền của mình.
    Task<List<DepositRequestResponse>> GetMyRequestsAsync(
        long userId);

    // User xem chi tiết một yêu cầu của mình.
    Task<DepositRequestResponse> GetByIdAsync(
        long userId,
        long requestId);

    // Admin duyệt hoặc từ chối yêu cầu.
    Task<DepositRequestResponse> UpdateStatusAsync(
        long adminUserId,
        long requestId,
        UpdateDepositRequestStatus request);
}