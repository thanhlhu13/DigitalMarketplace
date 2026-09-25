using System.Security.Claims;
using DigitalMarketplace.Application.DTOs.DepositRequests;
using DigitalMarketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalMarketplace.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepositRequestController : ControllerBase
{
    private readonly IDepositRequestService _depositRequestService;

    public DepositRequestController(
        IDepositRequestService depositRequestService)
    {
        _depositRequestService = depositRequestService;
    }

    // POST: api/DepositRequest
    // User tạo yêu cầu nạp tiền.
    [HttpPost]
    public async Task<ActionResult<DepositRequestResponse>> Create(
        CreateDepositRequest request)
    {
        var userId = GetCurrentUserId();

        var result = await _depositRequestService.CreateAsync(
            userId,
            request);

        return Ok(result);
    }

    // GET: api/DepositRequest
    // User xem lịch sử yêu cầu nạp tiền của mình.
    [HttpGet]
    public async Task<ActionResult<List<DepositRequestResponse>>> GetMyRequests()
    {
        var userId = GetCurrentUserId();

        var result = await _depositRequestService.GetMyRequestsAsync(
            userId);

        return Ok(result);
    }

    // GET: api/DepositRequest/{id}
    // User xem chi tiết yêu cầu của mình.
    [HttpGet("{id:long}")]
    public async Task<ActionResult<DepositRequestResponse>> GetById(
        long id)
    {
        var userId = GetCurrentUserId();

        var result = await _depositRequestService.GetByIdAsync(
            userId,
            id);

        return Ok(result);
    }

    // PUT: api/DepositRequest/{id}/status
    // Chỉ Admin mới được duyệt hoặc từ chối.
    [HttpPut("{id:long}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DepositRequestResponse>> UpdateStatus(
        long id,
        UpdateDepositRequestStatus request)
    {
        var adminUserId = GetCurrentUserId();

        var result = await _depositRequestService.UpdateStatusAsync(
            adminUserId,
            id,
            request);

        return Ok(result);
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