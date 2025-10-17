namespace App.Api.Contracts.Admins;

public sealed record PatchUserRequest(string? RoleName, bool? IsActive);