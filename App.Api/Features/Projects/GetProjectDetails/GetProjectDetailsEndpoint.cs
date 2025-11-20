using App.Application.Abstractions.Handlers;
using App.Application.Common.Dtos.Projects;
using App.Application.Common.Results;
using App.Application.Projects.Queries.GetProjectDetails;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Projects.GetProjectDetails;

public static class GetProjectDetailsEndpoint
{
    public static RouteGroupBuilder MapGetProjectDetailsEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", Handle)
            .WithSummary("Get full details for a single project")
            .Produces<GetProjectDetailsResponse>()
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid id,
        [FromServices] IQueryHandler<GetProjectDetailsQuery, Result<ProjectDetailsDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetProjectDetailsQuery(id), ct);

        if (!result.IsSuccess)
        {
            var error = result.Error!.Value;
            return error.Code switch
            {
                "not_found" => NotFound(new { message = error.Message }),
                _           => Problem(error.Message)
            };
        }

        var dto = result.Value!;
        var response = dto.ToResponse();

        return Ok(response);
    }

    private static GetProjectDetailsResponse ToResponse(this ProjectDetailsDto dto) =>
        new(
            Id:            dto.Id,
            Code:          dto.Code,
            ClientId:      dto.ClientId,
            ClientName:    dto.ClientName,
            ScopeId:       dto.ScopeId,
            ScopeName:     dto.ScopeName,
            Name:          dto.Name,
            Year:          dto.Year,
            Number:        dto.Number,
            Manager:       dto.Manager,
            Type:          dto.Type,
            Location:       dto.Location,
            CreatedAtUtc:  dto.CreatedAtUtc,
            UpdatedAtUtc:  dto.UpdatedAtUtc,
            DeletedAtUtc:  dto.DeletedAtUtc,
            CreatedById:   dto.CreatedById,
            UpdatedById:   dto.UpdatedById,
            DeletedBy: dto.DeletedById == Guid.Empty
                ? "Imported From Legacy Database"
                : dto.DeletedAtUtc.ToString()
        );
}