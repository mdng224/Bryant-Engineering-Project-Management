using App.Application.Abstractions;
using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Security;
using App.Application.Common;
using App.Application.Common.Results;
using App.Domain.Common;
using App.Domain.Security;
using App.Domain.Users;
using static App.Application.Common.R;

namespace App.Application.Auth.Queries.Login;

public sealed class LoginHandler(IUserReader users, IPasswordHasher hasher, ITokenService tokenService)
    : IQueryHandler<LoginQuery, Result<LoginResult>>
{
    public async Task<Result<LoginResult>> Handle(LoginQuery query, CancellationToken ct)
    {
        var normalizedEmail = query.Email.ToNormalizedEmail();
        var user = await users.GetByEmailAsync(normalizedEmail, ct);

        if (user is null)
            return Fail<LoginResult>("unauthorized", "Invalid credentials.");

        // Check account state
        if (user.Status != UserStatus.Active)
        {
            var message = user.Status switch
            {
                UserStatus.PendingEmail      => "Please verify your email before logging in.",
                UserStatus.PendingApproval   => "Your account is pending administrator approval.",
                UserStatus.Disabled          => "Your account has been disabled.",
                UserStatus.Denied            => "Your registration was denied by an administrator.",
                _                            => "Account is not active."
            };
            return Fail<LoginResult>("forbidden", message);
        }

        if (!hasher.Verify(query.Password, user.PasswordHash))
            return Fail<LoginResult>("unauthorized", "Invalid credentials.");

        var (token, exp) = tokenService.CreateForUser(user.Id, user.Email, user.RoleId.ToName());

        return Ok(new LoginResult(token, exp));
    }
}
