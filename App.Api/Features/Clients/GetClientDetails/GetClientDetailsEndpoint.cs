using App.Application.Abstractions.Handlers;
using App.Application.Clients.Queries.GetClientDetails;
using App.Application.Common.Dtos.Clients;
using App.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Clients.GetClientDetails;

public static class GetClientDetailsEndpoint
{
    public static RouteGroupBuilder MapGetClientDetailsEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", Handle)
            .WithSummary("Get full details for a single client")
            .Produces<GetClientDetailsResponse>()
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }
    
    private static async Task<IResult> Handle(
        [FromRoute] Guid id,
        [FromServices] IQueryHandler<GetClientDetailsQuery, Result<ClientDetailsDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetClientDetailsQuery(id), ct);

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
    
    private static GetClientDetailsResponse ToResponse(this ClientDetailsDto dto) =>
        new(
            Id:                  dto.Id,
            Name:                dto.Name,
            TotalActiveProjects: dto.TotalActiveProjects,
            TotalProjects:       dto.TotalProjects,
            CategoryName:        dto.CategoryName,
            TypeName:            dto.TypeName,
            CreatedAtUtc:        dto.CreatedAtUtc,
            UpdatedAtUtc:        dto.UpdatedAtUtc,
            DeletedAtUtc:        dto.DeletedAtUtc,
            CreatedById:         dto.CreatedById,
            UpdatedById:         dto.UpdatedById,
            DeletedById:         dto.DeletedById
        );
}