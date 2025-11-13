using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Repositories;
using App.Application.Common.Results;
using static App.Application.Common.R;

namespace App.Application.Users.Commands.DeleteUser;

public sealed class DeleteUserHandler(IUserRepository repository, IUnitOfWork uow)
    : ICommandHandler<DeleteUserCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteUserCommand command, CancellationToken ct)
    {
        var deleted = await repository.SoftDeleteAsync(command.Id, ct);
        
        if (!deleted)
            return Fail<Unit>(code: "not_found", "User not found.");

        await uow.SaveChangesAsync(ct);
        return Ok(Unit.Value);
    }
}