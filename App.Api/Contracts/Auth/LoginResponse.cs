namespace App.Api.Contracts.Auth;

public sealed record LoginResponse(string Token, DateTimeOffset ExpiresAtUtc);