namespace App.Application.Auth.Commands.VerifyEmail;

public enum VerifyEmailOutcome
{
    Ok,
    Invalid,      // token not found or user missing
    Expired,
    AlreadyUsed
}