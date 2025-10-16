using App.Application.Abstractions;
using App.Application.Common;
using App.Domain.Common;
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

        if (!user.IsActive)
            return Fail<LoginResult>("forbidden", "Account is not active.");

        if (!hasher.Verify(query.Password, user.PasswordHash))
            return Fail<LoginResult>("unauthorized", "Invalid credentials.");

        var (token, exp) = tokenService.CreateForUser(user.Id, user.Email, user.Role.Name);

        return Ok(new LoginResult(token, exp));
    }
}
