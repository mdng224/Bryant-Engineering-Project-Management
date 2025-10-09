using App.Application.Abstractions;
using BCryptNet = BCrypt.Net.BCrypt;

namespace App.Infrastructure.Auth;

public class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));

        // keep same work factor your existing rows use (11 in your DB)
        return BCryptNet.HashPassword(password, workFactor: 11);
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