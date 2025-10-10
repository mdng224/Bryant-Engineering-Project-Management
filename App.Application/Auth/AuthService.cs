using App.Application.Abstractions;
using App.Application.Common;

namespace App.Application.Auth;

public sealed class AuthService(
    IUserQueries queries,
    IUserCommands commands,
    IPasswordHasher hasher,
    ITokenService tokens
) : IAuthService
{
    public async Task<Result<LoginResult>> LoginAsync(LoginDto loginDto, CancellationToken ct)
    {
        var email = NormalizeEmail(loginDto.Email);
        var password = loginDto.Password;

        var user = await queries.GetByEmailAsync(email, ct);
        if (user is null)
            return Result<LoginResult>.Fail("unauthorized", "Invalid credentials.");

        if (!hasher.Verify(password, user.PasswordHash))
            return Result<LoginResult>.Fail("unauthorized", "Invalid credentials.");

        var (token, exp) = tokens.CreateForUser(user.Id, user.Email);

        return Result<LoginResult>.Success(new LoginResult(token, exp));
    }

    public async Task<Result<RegisterResult>> RegisterAsync(RegisterDto registerDto, CancellationToken ct)
    {
        var email = NormalizeEmail(registerDto.Email);
        var password = registerDto.Password;

        // Lookup normalized form only for existence check
        if (await queries.ExistsByEmailAsync(email, ct))
            return Result<RegisterResult>.Fail("conflict", "Email already registered.");

        var hash = hasher.Hash(password);
        var user = await commands.CreateAsync(email, hash, ct);

        return Result<RegisterResult>.Success(new RegisterResult(user.Id));
    }

    // Trim only — we store the user's original casing.
    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}