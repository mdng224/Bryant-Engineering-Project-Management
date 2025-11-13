using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos;
using App.Application.Common.Results;

namespace App.Application.Projects.Commands.RestoreProject;

public class RestoreProjectHandler(IProjectReader reader, IUnitOfWork uow)
    : ICommandHandler<RestoreProjectCommand, Result<ProjectListItemDto>>
{
    /*
    public async Task<Result<ProjectListItemDto>> Handle(RestoreProjectCommand cmd, CancellationToken ct)
    {
        
        var project = await reader.GetByIdAsync(cmd.Id, ct);
        if (project is null)
            return Fail<ProjectListItemDto>(code: "not_found", message: "Project not found.");

        if (!project.IsDeleted)  // Idempotent: already active
            return Ok(project.ToDto());

        if (!project.Restore())
            return Fail<ProjectListItemDto>(code: "restore_failed", message: "Project could not be restored.");
        
        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Another active row may now hold a unique Name/Code, etc.
            return Fail<ProjectListItemDto>(
                code: "conflict",
                message: "Restoring this project conflicts with an existing active project.");
        }

        return Ok(project.ToDto());
    }*/
    public Task<Result<ProjectListItemDto>> Handle(RestoreProjectCommand request, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}