namespace App.Api.Features.Auth.Login;

public sealed record LoginResponse(string Token, DateTimeOffset ExpiresAtUtc);