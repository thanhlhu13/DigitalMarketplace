using DigitalMarketplace.Application.DTOs.Auth;
using DigitalMarketplace.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace DigitalMarketplace.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);

        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(
        [FromBody] string refreshToken)
    {
        var response = await _authService.RefreshTokenAsync(
            refreshToken);

        return Ok(response);
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(
        [FromBody] string refreshToken)
    {
        await _authService.RevokeTokenAsync(refreshToken);

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (userIdClaim == null ||
            !long.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _authService.GetCurrentUserAsync(userId);

        return Ok(user);
    }
}