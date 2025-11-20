using App.Application.Abstractions.Handlers;
using App.Application.Clients.Queries.GetClientDetails;
using App.Application.Common.Dtos.Contacts;
using App.Application.Common.Results;
using App.Application.Contacts.Queries.GetContactDetails;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Contacts.GetContactDetails;

public static class GetContactDetailsEndpoint
{
    public static RouteGroupBuilder MapGetContactDetailsEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", Handle)
            .WithSummary("Get full details for a single contact")
            .Produces<GetContactDetailsResponse>()
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid id,
        [FromServices] IQueryHandler<GetContactDetailsQuery, Result<ContactDetailsDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetContactDetailsQuery(id), ct);

        if (!result.IsSuccess)
        {
            var error = result.Error!.Value;
            
            return error.Code switch
            {
                "not_found" => NotFound(new { message = error.Message }),
                _           => Problem(detail: error.Message)
            };
        }

        var dto = result.Value!;
        var response = dto.ToResponse();

        return Ok(response);
    }

    private static GetContactDetailsResponse ToResponse(this ContactDetailsDto dto) =>
        new(
            Id: dto.Id,
            Name: $"{dto.FirstName} {dto.LastName}",
            ClientId: dto.ClientId,
            NamePrefix: dto.NamePrefix,
            FirstName: dto.FirstName,
            MiddleName: dto.MiddleName,
            LastName: dto.LastName,
            NameSuffix: dto.NameSuffix,
            Company: dto.Company,
            Department: dto.Department,
            JobTitle: dto.JobTitle,
            AddressLine1: dto.AddressLine1,
            AddressLine2: dto.AddressLine2,
            AddressCity: dto.AddressCity,
            AddressState: dto.AddressState,
            AddressPostalCode: dto.AddressPostalCode,
            Country: dto.Country,
            BusinessPhone: dto.BusinessPhone,
            MobilePhone: dto.MobilePhone,
            PrimaryPhone: dto.PrimaryPhone,
            Email: dto.Email,
            WebPage: dto.WebPage,
            IsPrimaryForClient: dto.IsPrimaryForClient,
            CreatedAtUtc: dto.CreatedAtUtc,
            UpdatedAtUtc: dto.UpdatedAtUtc,
            DeletedAtUtc: dto.DeletedAtUtc,
            CreatedById: dto.CreatedById,
            UpdatedById: dto.UpdatedById,
            DeletedById: dto.DeletedById
        );
}
