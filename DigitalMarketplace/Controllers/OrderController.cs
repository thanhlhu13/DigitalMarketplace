using System.Security.Claims;
using DigitalMarketplace.Application.DTOs.Orders;
using DigitalMarketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalMarketplace.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Creates an order from the current user's cart.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create()
    {
        var userId = GetCurrentUserId();

        var order = await _orderService.CreateAsync(userId);

        return Ok(order);
    }

    /// <summary>
    /// Gets all orders belonging to the current user.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<OrderResponse>>> GetMyOrders()
    {
        var userId = GetCurrentUserId();

        var orders = await _orderService.GetMyOrdersAsync(userId);

        return Ok(orders);
    }

    /// <summary>
    /// Gets one order belonging to the current user.
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<OrderResponse>> GetById(long id)
    {
        var userId = GetCurrentUserId();

        var order = await _orderService.GetByIdAsync(
            userId,
            id);

        return Ok(order);
    }

    /// <summary>
    /// Updates an order status.
    /// Only Admin users are allowed to change order status.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> UpdateStatus(
        long id,
        UpdateOrderStatusRequest request)
    {
        await _orderService.UpdateStatusAsync(
            id,
            request);

        return NoContent();
    }

    /// <summary>
    /// Gets the ID of the currently authenticated user from JWT.
    /// </summary>
    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (userIdClaim == null ||
            !long.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException(
                "User is not authenticated.");
        }

        return userId;
    }

    [HttpPut("{id:long}/cancel")]
    public async Task<IActionResult> Cancel(long id)
    {
        var userId = GetCurrentUserId();

        await _orderService.CancelAsync(
            userId,
            id);

        return NoContent();
    }
}