using System.Security.Cryptography;
using System.Text;

namespace App.Infrastructure.Auth;

public static class TokenGenerator
{
    public static (string rawToken, string tokenHash) CreateTokenPair()
    {
        var rawBytes = RandomNumberGenerator.GetBytes(32);
        var rawToken = Convert.ToBase64String(rawBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
        
        return (rawToken, tokenHash);
    }

    public static string Hash(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}