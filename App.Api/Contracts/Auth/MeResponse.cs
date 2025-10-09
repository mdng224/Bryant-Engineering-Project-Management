namespace App.Api.Contracts.Auth;

public sealed record MeResponse(string Sub, string? Email);