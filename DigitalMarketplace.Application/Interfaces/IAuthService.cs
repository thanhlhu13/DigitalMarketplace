using DigitalMarketplace.Application.DTOs.Auth;

namespace DigitalMarketplace.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    Task<AuthResponse> LoginAsync(LoginRequest request);

    Task<AuthResponse> RefreshTokenAsync(string refreshToken);

    Task RevokeTokenAsync(string refreshToken);

    Task<object> GetCurrentUserAsync(long userId);
}