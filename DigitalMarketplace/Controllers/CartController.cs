using System.Security.Claims;
using DigitalMarketplace.Application.DTOs.Carts;
using DigitalMarketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalMarketplace.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Gets the cart of the currently authenticated user.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<CartResponse>> GetMyCart()
    {
        var userId = GetCurrentUserId();

        var cart = await _cartService.GetMyCartAsync(userId);

        return Ok(cart);
    }

    /// <summary>
    /// Adds a product to the current user's cart.
    /// </summary>
    [HttpPost("items")]
    public async Task<ActionResult<CartResponse>> AddItem(
        AddCartItemRequest request)
    {
        var userId = GetCurrentUserId();

        var cart = await _cartService.AddItemAsync(
            userId,
            request);

        return Ok(cart);
    }

    /// <summary>
    /// Updates the quantity of a cart item.
    /// </summary>
    [HttpPut("items/{cartItemId:long}")]
    public async Task<ActionResult<CartResponse>> UpdateItem(
        long cartItemId,
        UpdateCartItemRequest request)
    {
        var userId = GetCurrentUserId();

        var cart = await _cartService.UpdateItemAsync(
            userId,
            cartItemId,
            request);

        return Ok(cart);
    }

    /// <summary>
    /// Removes one item from the current user's cart.
    /// </summary>
    [HttpDelete("items/{cartItemId:long}")]
    public async Task<IActionResult> RemoveItem(long cartItemId)
    {
        var userId = GetCurrentUserId();

        await _cartService.RemoveItemAsync(
            userId,
            cartItemId);

        return NoContent();
    }

    /// <summary>
    /// Removes all items from the current user's cart.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetCurrentUserId();

        await _cartService.ClearCartAsync(userId);

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
}