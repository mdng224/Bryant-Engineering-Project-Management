// File: App.Application/Auth/Commands/VerifyEmail/VerifyEmailOutcomeExtensions.cs
namespace App.Application.Auth.Commands.VerifyEmail;

public static class VerifyEmailOutcomeExtensions
{
    public static int ToStatusCode(this string code) =>
        code switch
        {
            "invalid" => 400,
            "conflict" => 409,
            "expired" => 410,
            _ => 500
        };

    public static string ToName(this VerifyEmailOutcome outcome) =>
        outcome switch
        {
            VerifyEmailOutcome.Ok => "ok",
            VerifyEmailOutcome.Expired => "expired",
            VerifyEmailOutcome.AlreadyUsed => "used",
            _ => "invalid"
        };

    public static string ToMessage(this VerifyEmailOutcome outcome) =>
        outcome switch
        {
            VerifyEmailOutcome.Ok => "Your email has been verified successfully.",
            VerifyEmailOutcome.Expired => "This verification link has expired.",
            VerifyEmailOutcome.AlreadyUsed => "This link has already been used.",
            VerifyEmailOutcome.Invalid => "Invalid verification token.",
            _ => "Unexpected verification result."
        };
}