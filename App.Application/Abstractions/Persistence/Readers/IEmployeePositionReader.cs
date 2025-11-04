using App.Application.Common.Dtos;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IEmployeePositionReader
{
    Task<IReadOnlyDictionary<Guid, IReadOnlyList<PositionMiniDto>>> GetPositionsForEmployeesAsync(
        IReadOnlyCollection<Guid> employeeIds, CancellationToken ct);
}