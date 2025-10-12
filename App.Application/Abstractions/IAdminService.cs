
using App.Application.Admins;
using App.Application.Common;

namespace App.Application.Abstractions;

public interface IAdminService
{
    Task<Result<AdminUpdateResult>> UpdateUserAsync(
        Guid userId,
        string? roleName,   // null → don’t change
        bool? isActive,     // null → don’t change
        CancellationToken ct);
}
