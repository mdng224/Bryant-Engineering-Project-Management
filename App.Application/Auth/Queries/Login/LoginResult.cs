namespace App.Application.Auth.Queries.Login;

public sealed record LoginResult(string Token, DateTimeOffset ExpiresAtUtc);
