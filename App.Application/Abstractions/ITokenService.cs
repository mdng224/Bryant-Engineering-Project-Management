namespace App.Application.Abstractions;

public interface ITokenService
{
    (string token, DateTimeOffset expiresAtUtc) CreateForUser(Guid userId, string email, string roleName);
}