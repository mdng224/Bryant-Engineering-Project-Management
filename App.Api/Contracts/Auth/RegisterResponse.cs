namespace App.Api.Contracts.Auth;

public sealed record RegisterResponse(Guid UserId, string Status, string Message);