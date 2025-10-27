using App.Domain.Users;

namespace App.Application.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(Guid UserId, string? RoleName, UserStatus? Status);