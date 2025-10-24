using App.Application.Common;

namespace App.Application.Auth.Commands.VerifyEmail;

public sealed record VerifyEmailCommand(string Token);
