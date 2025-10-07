using App.Application.Abstractions;
using App.Application.Auth;
using App.Application.Common;

internal sealed class AuthService(
    IUserQueries queries,
    IUserCommands commands,
    IPasswordHasher hasher,
    ITokenService tokens
) : IAuthService
{
    public async Task<Result<LoginResult>> LoginAsync(LoginDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return Result<LoginResult>.Fail("invalid_request", "Email and password are required.");

        var user = await queries.GetByEmailAsync(dto.Email, ct);
        if (user is null)
            return Result<LoginResult>.Fail("unauthorized", "Invalid credentials.");

        if (!hasher.Verify(user.PasswordHash, dto.Password))
            return Result<LoginResult>.Fail("unauthorized", "Invalid credentials.");

        var (token, exp) = tokens.CreateForUser(user.Id, user.Email);

        return Result<LoginResult>.Success(new LoginResult(token, exp));
    }

    public async Task<Result<RegisterResult>> RegisterAsync(RegisterDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return Result<RegisterResult>.Fail("invalid_request", "Email and password are required.");

        if (await queries.ExistsByEmailAsync(dto.Email, ct))
            return Result<RegisterResult>.Fail("conflict", "Email already registered.");

        var hash = hasher.Hash(dto.Password);
        var user = await commands.CreateAsync(dto.Email, hash, ct);

        return Result<RegisterResult>.Success(new RegisterResult(user.Id));
    }
}
