using DigitalMarketplace.Application.DTOs.Auth;
using DigitalMarketplace.Domain.Entities;

namespace DigitalMarketplace.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(
        User user,
        IEnumerable<string> roles,
        out DateTime expiresAt);

    string GenerateRefreshToken();

    string HashRefreshToken(string refreshToken);

    JwtSettings GetSettings();
}