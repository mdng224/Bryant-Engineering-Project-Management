using App.Application.Abstractions;
using App.Application.Common;

namespace App.Application.Auth;

public sealed class AuthService(
    IUserQueries userQueries,
    IUserCommands userCommands,
    IPasswordHasher hasher,
    ITokenService tokens
) : IAuthService
{
    public async Task<Result<LoginResult>> LoginAsync(LoginDto loginDto, CancellationToken ct)
    {
        var email = NormalizeEmail(loginDto.Email);
        var password = loginDto.Password;

        var user = await userQueries.GetByEmailWithRoleAsync(email, ct);
        if (user is null || !hasher.Verify(password, user.PasswordHash))
            return Result<LoginResult>.Fail("unauthorized", "Invalid credentials.");

        if (!user.IsActive)
            return Result<LoginResult>.Fail("forbidden", "Account is not active. Approval required by administrator.");

        var (token, exp) = tokens.CreateForUser(user.Id, user.Email, user.Role.Name);

        return Result<LoginResult>.Success(new LoginResult(token, exp));
    }

    public async Task<Result<RegisterResult>> RegisterAsync(RegisterDto registerDto, CancellationToken ct)
    {
        var email = NormalizeEmail(registerDto.Email);
        var password = registerDto.Password;

        // Lookup normalized form only for existence check
        if (await userQueries.ExistsByEmailAsync(email, ct))
            return Result<RegisterResult>.Fail("conflict", "Email already registered.");

        var hash = hasher.Hash(password);
        var user = await userCommands.CreateAsync(email, hash, ct);

        return Result<RegisterResult>.Success(new RegisterResult(user.Id));
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}