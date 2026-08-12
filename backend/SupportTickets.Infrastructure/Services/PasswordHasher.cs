using Microsoft.AspNetCore.Identity;
using SupportTickets.Application.Common.Interfaces;
using SupportTickets.Domain.Entities;

namespace SupportTickets.Infrastructure.Services;

/// <summary>
/// Wraps ASP.NET Core Identity's battle-tested PBKDF2 hasher so the rest of the
/// app never touches raw passwords or a hashing algorithm directly.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _identityHasher = new();

    public string HashPassword(string password)
    {
        return _identityHasher.HashPassword(null!, password);
    }

    public bool VerifyPassword(string passwordHash, string providedPassword)
    {
        var result = _identityHasher.VerifyHashedPassword(null!, passwordHash, providedPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
