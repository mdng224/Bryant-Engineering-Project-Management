using App.Api.Contracts.Employees;
using App.Application.Common;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Employees.Queries;
using App.Domain.Common;

namespace App.Api.Features.Employees.Mappers;

internal static class EmployeeMappers
{
    public static GetEmployeesResponse ToGetEmployeesResponse(
        this PagedResult<EmployeeDto> pagedResult,
        IReadOnlyDictionary<Guid, string> positionLookup) =>
        new(
            Employees: pagedResult.Items.Select(dto => dto.ToListItem(positionLookup)).ToList(),
            TotalCount: pagedResult.TotalCount,
            Page: pagedResult.Page,
            PageSize: pagedResult.PageSize,
            TotalPages: pagedResult.TotalPages
        );

    public static GetEmployeesQuery ToQuery(this GetEmployeesRequest request)
    {
        var (page, pageSize) = request.PagedRequest;
        var normalizedName = (request.Name ?? string.Empty).ToNormalizedName();
        var pagedQuery = new PagedQuery(page, pageSize);
        
        return new GetEmployeesQuery(pagedQuery, normalizedName);
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
    
    private static EmployeeListItem ToListItem(this EmployeeDto dto, IReadOnlyDictionary<Guid, string> positionLookup) =>
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

    private static IReadOnlyList<string> ToPositionNames(
        this IEnumerable<Guid> positionIds, IReadOnlyDictionary<Guid, string> lookup) =>
        positionIds.Distinct().Select(id => lookup.TryGetValue(id, out var name) ? name : "Unknown").ToList();
}