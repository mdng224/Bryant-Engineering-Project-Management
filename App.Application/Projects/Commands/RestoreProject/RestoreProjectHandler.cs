using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Repositories;
using App.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
using static App.Application.Common.R;

namespace App.Application.Projects.Commands.RestoreProject;

public class RestoreProjectHandler(IProjectRepository repo, IUnitOfWork uow)
    : ICommandHandler<RestoreProjectCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RestoreProjectCommand cmd, CancellationToken ct)
    {
        
        var project = await repo.GetAsync(cmd.Id, ct);
        if (project is null)
            return Fail<Unit>(code: "not_found", message: "Project not found.");

        if (!project.IsDeleted)  // Idempotent: already active
            return Ok(Unit.Value);

        if (!project.Restore())
            return Fail<Unit>(code: "restore_failed", message: "Project could not be restored.");
        
        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Another active row may now hold a unique Name/Code, etc.
            return Fail<Unit>(
                code: "conflict",
                message: "Restoring this project conflicts with an existing active project.");
        }

        return Ok(Unit.Value);
    }
}