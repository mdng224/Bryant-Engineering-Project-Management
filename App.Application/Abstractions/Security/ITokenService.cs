namespace App.Application.Abstractions.Security;

public interface ITokenService
{
    (string token, DateTimeOffset expiresAtUtc) CreateForUser(Guid userId, string email, string roleName);
}