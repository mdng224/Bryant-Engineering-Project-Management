using App.Application.Auth;
using App.Application.Common;

namespace App.Application.Abstractions;

public interface IAuthService
{
    Task<Result<LoginResult>> LoginAsync(LoginDto dto, CancellationToken ct);
    Task<Result<RegisterResult>> RegisterAsync(RegisterDto dto, CancellationToken ct);

}