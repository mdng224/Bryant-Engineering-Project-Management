using App.Api.Contracts.Employees.Requests;
using App.Api.Contracts.Employees.Responses;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Employees.Queries;
using App.Domain.Common;

namespace App.Api.Features.Employees.Mappers;

internal static class EmployeeMappers
{
    public static GetEmployeesResponse ToGetEmployeesResponse(
        this PagedResult<EmployeeListItemDto> pagedResult)
    {
        var employees = pagedResult.Items.Select(dto => dto.ToListItem()).ToList();

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
        var getEmployeesQuery = new GetEmployeesQuery(pagedQuery, normalizedNameFilter, request.IsDeleted);
         
        return getEmployeesQuery;
    }
    
    public static EmployeeResponse ToEmployeeResponse(this EmployeeListItemDto item) =>
        new(item.Id,
            item.UserId,
            item.FirstName,
            item.LastName,
            item.PreferredName,
            item.PositionNames,
            item.EmploymentType,
            item.SalaryType,
            item.HireDate,
            item.EndDate,
            item.Department,
            item.CompanyEmail,
            item.WorkLocation,
            item.Notes,
            item.RecommendedRoleId,
            item.IsPreapproved,
            item.CreatedAtUtc,
            item.UpdatedAtUtc,
            item.DeletedAtUtc);

    private static EmployeeListItemResponse ToListItem(this EmployeeListItemDto item) =>
        new(
            Summary: item.ToSummaryResponse(),
            Details: item.ToEmployeeResponse());

    private static EmployeeSummaryResponse ToSummaryResponse(this EmployeeListItemDto item) =>
        new(item.Id,
            item.LastName,
            item.FirstName,
            item.PreferredName,
            item.PositionNames,
            item.Department,
            item.EmploymentType,
            item.HireDate,
            item.DeletedAtUtc);
}