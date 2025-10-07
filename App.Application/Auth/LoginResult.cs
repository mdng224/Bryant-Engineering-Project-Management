namespace App.Application.Auth;

public sealed record LoginResult(string Token, DateTimeOffset ExpiresAtUtc);
