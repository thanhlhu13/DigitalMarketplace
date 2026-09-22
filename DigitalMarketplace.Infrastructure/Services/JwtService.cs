using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DigitalMarketplace.Application.DTOs.Auth;
using DigitalMarketplace.Application.Interfaces;
using DigitalMarketplace.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DigitalMarketplace.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly JwtSettings _settings;

    public JwtService(IConfiguration configuration)
    {
        // Read JWT configuration from appsettings.json.
        _settings = configuration
            .GetSection("Jwt")
            .Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                "JWT configuration is missing.");
    }

    public string GenerateAccessToken(
        User user,
        IEnumerable<string> roles,
        out DateTime expiresAt)
    {
        expiresAt = DateTime.UtcNow
            .AddMinutes(_settings.AccessTokenExpirationMinutes);

        var claims = new List<Claim>
    {
        // Unique identifier of the authenticated user.
        // NameIdentifier is used by ASP.NET Core to identify the current user.
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),

        // Username used by the application.
        new(ClaimTypes.Name, user.Username),

        // Email of the authenticated user.
        new(ClaimTypes.Email, user.Email),

        // JWT ID allows each token to have a unique identifier.
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        // Add user roles to the JWT.
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_settings.Key));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        // Generate a cryptographically secure random token.
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(randomBytes);
    }

    public string HashRefreshToken(string refreshToken)
    {
        // Only the hash is stored in the database.
        // The original refresh token is returned to the client.
        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(refreshToken));

        return Convert.ToHexString(hash);
    }

    public JwtSettings GetSettings()
    {
        return _settings;
    }
}