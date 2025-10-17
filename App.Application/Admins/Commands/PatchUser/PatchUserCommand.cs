namespace App.Application.Admins.Commands.PatchUser;

public sealed record PatchUserCommand(Guid UserId, string? RoleName, bool? IsActive);