using App.Api.Contracts.Employees;
using App.Application.Common;
using App.Application.Common.Dtos;
using App.Application.Employees.Queries;
using App.Domain.Common;

namespace App.Api.Features.Employees.Mappers;

internal static class EmployeeMappers
{
    public static GetEmployeesResponse ToGetEmployeesResponse(
        this GetEmployeesResult result,
        IReadOnlyDictionary<Guid, string> positionLookup) =>
        new(
            Employees: result.EmployeesDtos.Select(dto => dto.ToListItem(positionLookup)).ToList(),
            TotalCount: result.TotalCount,
            Page: result.Page,
            PageSize: result.PageSize,
            TotalPages: result.TotalPages
        );

    public static GetEmployeesQuery ToQuery(this GetEmployeesRequest request)
    {
        var page = request.Page is >= 1
            ? request.Page
            : PagingDefaults.DefaultPage;
        var size = request.PageSize is >= 1 and <= PagingDefaults.MaxPageSize
            ? request.PageSize
            : PagingDefaults.DefaultPageSize;

        var normalizedName = request.Name?.ToNormalizedName();

        return new GetEmployeesQuery(page, size, normalizedName);
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