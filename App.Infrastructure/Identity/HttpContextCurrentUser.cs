using System.Security.Claims;
using App.Application.Abstractions.Security;
using Microsoft.AspNetCore.Http;

namespace App.Infrastructure.Identity;

/// <summary>
/// Resolves the current authenticated user from the active HTTP context.
/// Used by persistence interceptors and other infrastructure services.
/// </summary>
public sealed class HttpContextCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var user = accessor.HttpContext?.User;
            if (user is null || !user.Identity?.IsAuthenticated == true)
                return null;

            // Commonly stored in ClaimTypes.NameIdentifier or "sub"
            var idValue = user.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? user.FindFirstValue("sub");

            return Guid.TryParse(idValue, out var id) ? id : null;
        }
    }
}