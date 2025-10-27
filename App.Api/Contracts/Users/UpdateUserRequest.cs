using App.Domain.Users;

namespace App.Api.Contracts.Users;

public sealed record UpdateUserRequest(string? RoleName, UserStatus? Status);