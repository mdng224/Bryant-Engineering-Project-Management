using App.Application.Abstractions;
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

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCryptNet.Verify(password, hash.Trim());
        }
        catch
        {
            // Catch SaltParseException and anything else → treat as invalid
            return false;
        }
    }
}