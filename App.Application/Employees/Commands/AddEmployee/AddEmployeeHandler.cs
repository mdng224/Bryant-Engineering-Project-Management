using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Exceptions;
using App.Application.Abstractions.Persistence.Repositories;
using App.Application.Common.Results;
using App.Application.Employees.Mappers;
using static App.Application.Common.R;

namespace App.Application.Employees.Commands.AddEmployee;

public class AddEmployeeHandler(IEmployeeRepository repository, IUnitOfWork uow)
    : ICommandHandler<AddEmployeeCommand, Result<Guid>>
{
    private const string CodeUserLinked  = "Employee.UserAlreadyLinked";
    private const string MessageUserLinked   = "The selected user is already linked to another employee.";

    private const string CodeEmailTaken  = "Employee.CompanyEmailConflict";
    private const string MessageEmailTaken   = "Another active employee already uses this company email address.";

    private const string CodeUnique      = "Employee.UniqueConstraintConflict";
    private const string MessageUnique       = "An employee with the same unique values already exists.";

    public async Task<Result<Guid>> Handle(AddEmployeeCommand command, CancellationToken ct)
    {
        var employee = command.ToDomain();

        try
        {
            repository.Add(employee);
            await uow.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintViolationException ex)
        {
            return ex.ConstraintName switch
            {
                "ux_employees_user_id"       => Fail<Guid>(CodeUserLinked,  MessageUserLinked),
                "ux_employees_company_email" => Fail<Guid>(CodeEmailTaken,  MessageEmailTaken),
                _                            => Fail<Guid>(CodeUnique,      MessageUnique)
            };
        }

        return Ok(employee.Id);
    }
}