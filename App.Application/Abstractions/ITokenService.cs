namespace App.Application.Abstractions;

public interface ITokenService
{
    // return compact JWT + expiry
    (string token, DateTimeOffset expiresAtUtc) CreateForUser(Guid userId, string email);
}