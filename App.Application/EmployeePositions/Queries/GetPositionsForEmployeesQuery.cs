namespace App.Application.EmployeePositions.Queries;

public sealed record GetPositionsForEmployeesQuery(IReadOnlyCollection<Guid> EmployeeIds);