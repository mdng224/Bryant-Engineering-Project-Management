namespace App.Application.Admins.Commands.UpdateUser;

public sealed record UpdateUserCommand(Guid UserId, string? RoleName, bool? IsActive);