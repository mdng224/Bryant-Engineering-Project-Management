namespace App.Api.Features.Auth.Register;

public sealed record RegisterResponse(Guid UserId, string Status, string Message);