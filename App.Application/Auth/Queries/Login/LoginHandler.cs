using App.Application.Abstractions;
using App.Application.Common;

namespace App.Application.Auth.Queries.Login;

public sealed class LoginHandler(IUserReader users, IPasswordHasher hasher, ITokenService tokenService)
    : IQueryHandler<LoginQuery, Result<LoginResult>>
{
    public async Task<Result<LoginResult>> Handle(LoginQuery query, CancellationToken ct)
    {
        var normalizedEmail = query.Email.Trim().ToLowerInvariant();
        var user = await users.GetByEmailAsync(normalizedEmail, ct);

        if (user is null)
            return Result<LoginResult>.Fail("unauthorized", "Invalid credentials.");

        if (!user.IsActive)
            return Result<LoginResult>.Fail("forbidden", "Account is not active.");

        if (!hasher.Verify(query.Password, user.PasswordHash))
            return Result<LoginResult>.Fail("unauthorized", "Invalid credentials.");

        var (token, exp) = tokenService.CreateForUser(user.Id, user.Email, user.Role.Name);

        return Result<LoginResult>.Success(new LoginResult(token, exp));
    }
}
