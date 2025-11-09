using App.Application.Common.Dtos;
using App.Domain.Projects;

namespace App.Application.Projects.Mappers;

public static class ProjectMappers
{
    public static ProjectDto ToDto(this Project project)
    {
        var isLegacyDb = project.DeletedById == Guid.Empty;
        return new ProjectDto(
            Id: project.Id,
            ClientId: project.ClientId,
            ClientName: project.Client.Name ?? "unknown",
            Name: project.Name,
            Code: project.Code,
            Year: project.Year,
            Number: project.Number,
            NewCode: project.NewCode,
            Scope: project.Scope,
            Manager: project.Manager,
            Type: project.Type,
            Address: project.Address,
            CreatedAtUtc: project.CreatedAtUtc,
            UpdatedAtUtc: project.UpdatedAtUtc,
            DeletedAtUtc: project.DeletedAtUtc,
            CreatedById: project.CreatedById,
            UpdatedById: project.UpdatedById,
            DeletedBy: isLegacyDb ? "Imported From Legacy Database" : null
        );
    }
}