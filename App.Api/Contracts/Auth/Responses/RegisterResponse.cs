namespace App.Api.Contracts.Auth.Responses;

public sealed record RegisterResponse(Guid UserId, string Status, string Message);