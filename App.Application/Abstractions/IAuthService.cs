using App.Application.Auth;
using App.Application.Common;

namespace App.Application.Abstractions;

public interface IAuthService
{
    Task<Result<LoginResult>> LoginAsync(LoginDto loginDto, CancellationToken ct);
    Task<Result<RegisterResult>> RegisterAsync(RegisterDto registerDto, CancellationToken ct);

}