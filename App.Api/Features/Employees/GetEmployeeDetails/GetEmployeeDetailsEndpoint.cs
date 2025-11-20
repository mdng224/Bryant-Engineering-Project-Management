using App.Application.Abstractions.Handlers;
using App.Application.Common.Dtos.Employees;
using App.Application.Common.Results;
using App.Application.Employees.Queries.GetEmployeeDetails;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Employees.GetEmployeeDetails;

public static class GetEmployeeDetailsEndpoint
{
     public static RouteGroupBuilder MapGetEmployeeDetailsEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", Handle)
            .WithSummary("Get full details for a single empoyee")
            .Produces<GetEmployeeDetailsResponse>()
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid id,
        [FromServices] IQueryHandler<GetEmployeeDetailsQuery, Result<EmployeeDetailsDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetEmployeeDetailsQuery(id), ct);

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

    private static GetEmployeeDetailsResponse ToResponse(this EmployeeDetailsDto dto) =>
        new(
            Id:               dto.Id,
            UserId:           dto.UserId,
            FirstName:        dto.FirstName,
            LastName:         dto.LastName,
            PreferredName:    dto.PreferredName,
            FullName:         $"{dto.FirstName} {dto.LastName}",
            EmploymentType:   dto.EmploymentType,
            SalaryType:       dto.SalaryType,
            Department:       dto.Department,
            HireDate:         dto.HireDate,
            EndDate:          dto.EndDate,
            CompanyEmail:     dto.CompanyEmail,
            WorkLocation:     dto.WorkLocation,
            Notes:            dto.Notes,
            PositionNames:    dto.PositionNames,
            AddressLine1:     dto.AddressLine1,
            AddressLine2:     dto.AddressLine2,
            City:             dto.City,
            State:            dto.State,
            PostalCode:       dto.PostalCode,
            RecommendedRole: dto.RecommendRole,
            IsPreapproved:    dto.IsPreapproved,
            CreatedAtUtc:     dto.CreatedAtUtc,
            UpdatedAtUtc:     dto.UpdatedAtUtc,
            DeletedAtUtc:     dto.DeletedAtUtc,
            CreatedById:      dto.CreatedById,
            UpdatedById:      dto.UpdatedById,
            DeletedBy: dto.DeletedById == Guid.Empty
                ? "Imported From Legacy Database"
                : dto.DeletedAtUtc.ToString()
        );
}