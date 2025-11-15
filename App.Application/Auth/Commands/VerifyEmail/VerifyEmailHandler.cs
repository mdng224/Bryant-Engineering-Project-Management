using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Abstractions.Persistence.Repositories;
using App.Application.Common.Results;
using App.Domain.Auth;
using App.Domain.Common;
using App.Domain.Users;
using static App.Application.Common.R;

namespace App.Application.Auth.Commands.VerifyEmail;

public sealed class VerifyEmailHandler(
    IEmailVerificationRepository emailVerificationRepo,
    IEmployeeReader employeeReader,
    IUserRepository userRepo,
    IUnitOfWork uow) : ICommandHandler<VerifyEmailCommand, Result<VerifyEmailResult>>
{
    public async Task<Result<VerifyEmailResult>> Handle(VerifyEmailCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
            return Ok(new VerifyEmailResult(VerifyEmailOutcome.Invalid));

        var verification = await emailVerificationRepo.GetForUpdateByTokenHashAsync(command.Token, ct);
        
        var outcome = DetermineOutcome(verification, DateTime.UtcNow);
        if (outcome is not VerifyEmailOutcome.Ok)
            return Ok(new VerifyEmailResult(outcome));
        
        var user = await userRepo.GetForUpdateAsync(verification!.UserId, ct);
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

        await uow.SaveChangesAsync(ct);
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