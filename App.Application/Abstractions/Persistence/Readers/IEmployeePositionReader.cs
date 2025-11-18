using App.Application.Common.Dtos;
using App.Application.Common.Dtos.Projects;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IEmployeePositionReader
{
    Task<IReadOnlyDictionary<Guid, IReadOnlyList<PositionMiniDto>>> GetPositionsForEmployeesAsync(
        IReadOnlyCollection<Guid> employeeIds, CancellationToken ct);
}