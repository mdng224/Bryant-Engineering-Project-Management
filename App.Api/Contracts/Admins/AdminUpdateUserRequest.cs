namespace App.Api.Contracts.Admins;

public sealed record AdminUpdateUserRequest(string? RoleName, bool? IsActive);