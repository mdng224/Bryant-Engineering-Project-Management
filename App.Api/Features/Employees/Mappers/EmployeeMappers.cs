using App.Api.Contracts.Employees;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Employees.Queries;
using App.Domain.Common;

namespace App.Api.Features.Employees.Mappers;

internal static class EmployeeMappers
{
    public static GetEmployeesResponse ToGetEmployeesResponse(
        this PagedResult<EmployeeDto> pagedResult,
        IReadOnlyDictionary<Guid, IReadOnlyList<PositionMiniDto>> positionsByEmployee)
    {
        var employees = pagedResult.Items
            .Select(dto =>
            {
                positionsByEmployee.TryGetValue(dto.Id, out var positions);
                return dto.ToListItem(positions ?? []);
            })
            .ToList();

        return new GetEmployeesResponse(
            Employees: employees,
            TotalCount: pagedResult.TotalCount,
            Page:  pagedResult.Page,
            PageSize: pagedResult.PageSize,
            TotalPages: pagedResult.TotalPages
        );
    }
        
    public static GetEmployeesQuery ToQuery(this GetEmployeesRequest request)
    {
        var normalizedNameFilter = (request.NameFilter ?? string.Empty).ToNormalizedName();
        var pagedQuery = new PagedQuery(request.Page, request.PageSize);
         var getEmployeesQuery = new GetEmployeesQuery(pagedQuery, normalizedNameFilter);
         
         return getEmployeesQuery;
    }

    private static EmployeeResponse
        ToEmployeeResponse(this EmployeeDto dto, IReadOnlyList<string> positionNames) =>
        new(dto.Id,
            dto.UserId,
            dto.FirstName,
            dto.LastName,
            dto.PreferredName,
            dto.EmploymentType,
            dto.SalaryType,
            dto.HireDate,
            dto.EndDate,
            dto.Department,
            dto.CompanyEmail,
            dto.WorkLocation,
            dto.Notes,
            dto.RecommendedRoleId,
            dto.IsPreapproved,
            positionNames,
            dto.CreatedAtUtc,
            dto.UpdatedAtUtc,
            dto.DeletedAtUtc,
            dto.IsActive);
    
    private static EmployeeListItem ToListItem(this EmployeeDto dto, IReadOnlyList<PositionMiniDto> positionLookup) =>
        new(
            Summary: dto.ToSummaryResponse(),
            Details: dto.ToEmployeeResponse(dto.PositionIds.ToPositionNames(positionLookup))
        );

    private static EmployeeSummaryResponse ToSummaryResponse(this EmployeeDto dto) =>
        new(dto.Id,
            dto.LastName,
            dto.FirstName,
            dto.PreferredName,
            dto.Department,
            dto.EmploymentType,
            dto.HireDate,
            dto.IsActive);

    private static List<string> ToPositionNames(
        this IEnumerable<Guid> positionIds,
        IReadOnlyList<PositionMiniDto> employeePositions)
    {
        // Build a quick lookup: PositionId -> Name
        var map = employeePositions.Count == 0
            ? new Dictionary<Guid, string>()
            : employeePositions.ToDictionary(p => p.Id, p => p.Name);

        return positionIds
            .Distinct()
            .Select(id => map.TryGetValue(id, out var name) ? name : "Unknown")
            .ToList();
    }
}