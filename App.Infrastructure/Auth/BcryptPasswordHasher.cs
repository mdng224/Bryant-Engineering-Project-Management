using App.Application.Abstractions.Security;
using BCryptNet = BCrypt.Net.BCrypt;

namespace App.Infrastructure.Auth;

public class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return string.IsNullOrEmpty(password)
            ? throw new ArgumentException("Password cannot be null or empty.", nameof(password))
            : BCryptNet.HashPassword(password, workFactor: 11); // keep same work factor your existing rows use (11 in your DB)
    }

    public bool Verify(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrWhiteSpace(providedPassword))
            return false;

        try
        {
            return BCryptNet.Verify(hashedPassword, providedPassword.Trim());
        }
        catch
        {
            // Catch SaltParseException and anything else → treat as invalid
            return false;
        }
    }
}