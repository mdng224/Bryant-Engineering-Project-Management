using App.Domain.Users;

namespace App.Api.Features.Users.UpdateUser;

public sealed record UpdateUserRequest(string? RoleName, UserStatus? Status);