using DigitalMarketplace.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace DigitalMarketplace.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _passwordHasher = new();

    public string Hash(string password)
    {
        // Hash the password before storing it in the database.
        return _passwordHasher.HashPassword(null!, password);
    }

    public bool Verify(string password, string passwordHash)
    {
        // Verify the provided password against the stored password hash.
        var result = _passwordHasher.VerifyHashedPassword(
            null!,
            passwordHash,
            password);

        return result == PasswordVerificationResult.Success ||
               result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}