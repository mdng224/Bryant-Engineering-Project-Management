using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Repositories;
using App.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
using static App.Application.Common.R;

namespace App.Application.Employees.Commands.RestoreEmployee;

public class RestoreEmployeeHandler(IEmployeeRepository repo, IUnitOfWork uow)
    : ICommandHandler<RestoreEmployeeCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RestoreEmployeeCommand command, CancellationToken ct)
    {
        var employee = await repo.GetAsync(command.Id, ct);
        if (employee is null)
            return Fail<Unit>(code: "not_found", message: "employee not found.");

        if (!employee.IsDeleted)  // Idempotent: already active
            return Ok(Unit.Value);

        if (!employee.Restore())
            return Fail<Unit>(code: "restore_failed", message: "employee could not be restored.");
        
        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Another active row may now hold a unique Name/Code, etc.
            return Fail<Unit>(
                code: "conflict",
                message: "Restoring this employee conflicts with an existing active employee.");
        }

        return Ok(Unit.Value);
    }
}