namespace App.Api.Contracts.Users;

public sealed record UpdateUserRequest(string? RoleName, bool? IsActive);