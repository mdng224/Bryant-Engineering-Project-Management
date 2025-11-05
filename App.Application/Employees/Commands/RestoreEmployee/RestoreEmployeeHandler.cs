using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos;
using App.Application.Common.Results;
using App.Application.Employees.Mappers;
using Microsoft.EntityFrameworkCore;
using static App.Application.Common.R;

namespace App.Application.Employees.Commands.RestoreEmployee;

public class RestoreEmployeeHandler(IEmployeeReader reader, IUnitOfWork uow)
    : ICommandHandler<RestoreEmployeeCommand, Result<EmployeeDto>>
{
    public async Task<Result<EmployeeDto>> Handle(RestoreEmployeeCommand cmd, CancellationToken ct)
    {
        var employee = await reader.GetByIdAsync(cmd.Id, ct);
        if (employee is null)
            return Fail<EmployeeDto>(code: "not_found", message: "employee not found.");

        if (!employee.IsDeleted)  // Idempotent: already active
            return Ok(employee.ToDto());

        if (!employee.Restore())
            return Fail<EmployeeDto>(code: "restore_failed", message: "employee could not be restored.");
        
        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Another active row may now hold a unique Name/Code, etc.
            return Fail<EmployeeDto>(
                code: "conflict",
                message: "Restoring this employee conflicts with an existing active employee.");
        }

        return Ok(employee.ToDto());
    }
}