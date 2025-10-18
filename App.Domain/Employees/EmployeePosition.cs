using App.Domain.Common;

namespace App.Domain.Employees;

public sealed class EmployeePosition
{
    // --- Keys / FKs ---------------------------------------------------------
    public Guid Id { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public Guid PositionId { get; private set; }
    public Position Position { get; private set; } = null!;
    
    // --- Constructors -------------------------------------------------------
    private EmployeePosition() { }
    public EmployeePosition(Guid employeeId, Guid positionId)
    {
        Id = Guid.CreateVersion7();
        EmployeeId = Guard.AgainstDefault(employeeId, nameof(employeeId));
        PositionId = Guard.AgainstDefault(positionId, nameof(positionId));
    }

    // --- Mutators -----------------------------------------------------------
    public void AssignPosition(Guid positionId)
    {
        PositionId = Guard.AgainstDefault(positionId, nameof(positionId));
    }
}