namespace App.Domain.Employees;
public sealed class Employee
{
    public Guid UserId { get; private set; }                 // PK & FK to Users
    public string? JobTitle { get; private set; }
    public DateTimeOffset? HireDate { get; private set; }

    // nav
    public Users.User User { get; private set; } = null!;

    private Employee() { }
    public Employee(Guid userId, string? jobTitle = null, DateTimeOffset? hireDate = null)
        => (UserId, JobTitle, HireDate) = (userId, jobTitle, hireDate);
}