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

        if (HasMissingCredentials(email, password))
            return Result<LoginResult>.Fail("invalid_request", "Email and password are required.");

        // Query normalized version (lowercase for case-insensitive lookup)
        var user = await queries.GetByEmailAsync(email.ToLowerInvariant(), ct);
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

        if (HasMissingCredentials(email, password))
            return Result<RegisterResult>.Fail("invalid_request", "Email and password are required.");

        // Lookup normalized form only for existence check
        if (await queries.ExistsByEmailAsync(email.ToLowerInvariant(), ct))
            return Result<RegisterResult>.Fail("conflict", "Email already registered.");

        var hash = hasher.Hash(password);

        // Store trimmed, original-casing version
        var user = await commands.CreateAsync(email, hash, ct);

        return Result<RegisterResult>.Success(new RegisterResult(user.Id));
    }

    private static bool HasMissingCredentials(string email, string? password) =>
        string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password);

    // Trim only — we store the user's original casing.
    private static string NormalizeEmail(string? email) => (email ?? string.Empty).Trim();
}