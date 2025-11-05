namespace App.Api.Contracts.Auth.Responses;

public sealed record LoginResponse(string Token, DateTimeOffset ExpiresAtUtc);