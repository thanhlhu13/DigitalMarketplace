using DigitalMarketplace.Application.DTOs.Auth;
using DigitalMarketplace.Application.Interfaces;
using DigitalMarketplace.Domain.Entities;
using DigitalMarketplace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalMarketplace.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthService(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check whether the username is already registered.
        var usernameExists = await _context.Users
            .AnyAsync(x => x.Username == request.Username);

        if (usernameExists)
        {
            throw new InvalidOperationException(
                "Username is already registered.");
        }

        // Check whether the email is already registered.
        var emailExists = await _context.Users
            .AnyAsync(x => x.Email == request.Email);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "Email is already registered.");
        }

        // Find the default role assigned to newly registered users.
        var userRole = await _context.Roles
            .FirstOrDefaultAsync(x => x.Name == "User");

        if (userRole == null)
        {
            throw new InvalidOperationException(
                "Default User role does not exist.");
        }

        // Start a transaction so the whole registration process
        // succeeds or rolls back as one unit.
        await using var transaction = await _context.Database
            .BeginTransactionAsync();

        try
        {
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = _passwordHasher.Hash(request.Password),
                FullName = request.FullName,
                Phone = request.Phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            // Save so SQL Server generates the user's Id.
            await _context.SaveChangesAsync();

            var userRoleEntity = new UserRole
            {
                UserId = user.Id,
                RoleId = userRole.Id
            };

            _context.UserRoles.Add(userRoleEntity);

            await _context.SaveChangesAsync();

            // Generate tokens after the user and role have been created.
            var roles = new[] { userRole.Name };

            var accessToken = _jwtService.GenerateAccessToken(
                user,
                roles,
                out var accessTokenExpiresAt);

            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = _jwtService.HashRefreshToken(refreshToken),
                ExpiresAt = DateTime.UtcNow.AddDays(
                    _jwtService.GetSettings().RefreshTokenExpirationDays),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshTokenEntity);

            await _context.SaveChangesAsync();

            // All operations succeeded, so permanently commit them.
            await transaction.CommitAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = accessTokenExpiresAt,
                RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt
            };
        }
        catch
        {
            // If any step fails, undo all database changes
            // made during this registration.
            await transaction.RollbackAsync();

            throw;
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Find the user by email.
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == request.Email);

        // Do not reveal whether the email exists.
        if (user == null || !_passwordHasher.Verify(
            request.Password,
            user.PasswordHash))
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        // Make sure the account is still active.
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "User account is inactive.");
        }

        // Load all roles assigned to the user.
        var roles = await (
            from userRole in _context.UserRoles
            join role in _context.Roles
                on userRole.RoleId equals role.Id
            where userRole.UserId == user.Id
            select role.Name
        ).ToListAsync();

        // Generate a short-lived access token.
        var accessToken = _jwtService.GenerateAccessToken(
            user,
            roles,
            out var accessTokenExpiresAt);

        // Generate a long-lived refresh token.
        var refreshToken = _jwtService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _jwtService.HashRefreshToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(
                _jwtService.GetSettings().RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshTokenEntity);

        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        // Hash the refresh token received from the client.
        // The database stores only the hash, not the original token.
        var tokenHash = _jwtService.HashRefreshToken(refreshToken);

        // Find an active refresh token that has not expired or been revoked.
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x =>
                x.TokenHash == tokenHash &&
                x.RevokedAt == null &&
                x.ExpiresAt > DateTime.UtcNow);

        if (storedToken == null)
        {
            throw new UnauthorizedAccessException(
                "Invalid or expired refresh token.");
        }

        // Load the user associated with the refresh token.
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == storedToken.UserId);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "User account is inactive or does not exist.");
        }

        // Load all roles assigned to the user.
        var roles = await (
            from userRole in _context.UserRoles
            join role in _context.Roles
                on userRole.RoleId equals role.Id
            where userRole.UserId == user.Id
            select role.Name
        ).ToListAsync();

        // Generate a new access token.
        var accessToken = _jwtService.GenerateAccessToken(
            user,
            roles,
            out var accessTokenExpiresAt);

        // Rotate the refresh token.
        // The old refresh token can no longer be used.
        storedToken.RevokedAt = DateTime.UtcNow;

        var newRefreshToken = _jwtService.GenerateRefreshToken();

        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _jwtService.HashRefreshToken(newRefreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(
                _jwtService.GetSettings().RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(newRefreshTokenEntity);

        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshTokenExpiresAt = newRefreshTokenEntity.ExpiresAt
        };
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        // Hash the refresh token received from the client
        // so we can find the stored token without storing
        // the original token in the database.
        var tokenHash = _jwtService.HashRefreshToken(refreshToken);

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x =>
                x.TokenHash == tokenHash &&
                x.RevokedAt == null);

        if (storedToken == null)
        {
            throw new UnauthorizedAccessException(
                "Invalid or already revoked refresh token.");
        }

        // Mark the refresh token as revoked.
        // The token remains in the database for audit/history purposes.
        storedToken.RevokedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<object> GetCurrentUserAsync(long userId)
    {
        var user = await _context.Users
            .Where(x => x.Id == userId)
            .Select(x => new
            {
                x.Id,
                x.Username,
                x.Email,
                x.FullName,
                x.Phone,
                x.IsActive,
                x.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            throw new UnauthorizedAccessException(
                "User does not exist.");
        }

        return user;
    }

}