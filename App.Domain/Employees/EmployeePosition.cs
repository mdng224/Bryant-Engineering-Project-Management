using App.Domain.Common;

namespace App.Domain.Employees;

public sealed class EmployeePosition
{
    // --- Constructors -------------------------------------------------------
    private EmployeePosition() { }
    public EmployeePosition(Guid employeeId, Guid positionId)
    {
        EmployeeId = Guard.AgainstDefault(employeeId, nameof(employeeId));
        PositionId = Guard.AgainstDefault(positionId, nameof(positionId));
    }
    
    // --- Composite Key / FKs -----------------------------------------------
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public Guid PositionId { get; private set; }
    public Position Position { get; private set; } = null!;
    
    // --- Mutators -----------------------------------------------------------
    public void AssignPosition(Guid positionId)
    {
        PositionId = Guard.AgainstDefault(positionId, nameof(positionId));
    }
}