using System.Security.Cryptography;
using System.Text;
using App.Application.Abstractions;
using App.Application.Common;
using App.Domain.Auth;
using App.Domain.Common;
using App.Domain.Users;
using static App.Application.Common.R;

namespace App.Application.Auth.Commands.VerifyEmail;

public sealed class VerifyEmailHandler(
    IEmailVerificationReader emailVerificationReader,
    IEmailVerificationWriter emailVerificationWriter,
    IUserReader userReader,
    IUserWriter userWriter,
    IEmployeeReader employeeReader) : ICommandHandler<VerifyEmailCommand, Result<VerifyEmailResult>>
{
    public async Task<Result<VerifyEmailResult>> Handle(VerifyEmailCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
            return Ok(new VerifyEmailResult(VerifyEmailOutcome.Invalid));

        var verification = await emailVerificationReader.GetByTokenHashAsync(command.Token, ct);
        
        var outcome = DetermineOutcome(verification, DateTime.UtcNow);
        if (outcome is not VerifyEmailOutcome.Ok)
            return Ok(new VerifyEmailResult(outcome));
        
        var user = await userReader.GetByIdAsync(verification!.UserId, ct);
        if (user is null)
            return Ok(new VerifyEmailResult(VerifyEmailOutcome.Invalid));

        // ✅ Step 1: mark the user as email verified
        if (user.Status == UserStatus.PendingEmail)
        {
            user.MarkEmailVerified();
            await HandleUserActivationAsync(user, employeeReader, ct);
        }

        // ✅ Step 2: mark the token as used
        verification.MarkUsed();

        // TODO: Implement unit of work to ensure atomic user+verification updates
        await userWriter.UpdateAsync(user, ct);
        await emailVerificationWriter.UpdateAsync(verification, ct);

        return Ok(new VerifyEmailResult(VerifyEmailOutcome.Ok));
    }
    
    // ------------------- Helpers -------------------
    
    private static VerifyEmailOutcome DetermineOutcome(EmailVerification? verification, DateTime nowUtc)
    {
        return verification switch
        {
            null                                      => VerifyEmailOutcome.Invalid,
            { Used: true }                            => VerifyEmailOutcome.AlreadyUsed,
            { ExpiresAtUtc: var exp } when exp <= nowUtc => VerifyEmailOutcome.Expired,
            _                                         => VerifyEmailOutcome.Ok
        };
    }
    
    private static async Task HandleUserActivationAsync(User user, IEmployeeReader employeeReader, CancellationToken ct)
    {
        if (user.Status != UserStatus.PendingEmail)
            return;

        var employee = await employeeReader.GetByCompanyEmailAsync(user.Email.ToNormalizedEmail(), ct);

        if (employee is null)
            user.MarkPendingApproval();
        else
            user.Activate();
    }
}