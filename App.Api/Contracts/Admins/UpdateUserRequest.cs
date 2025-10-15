namespace App.Api.Contracts.Admins;

public sealed record UpdateUserRequest(string? RoleName, bool? IsActive);