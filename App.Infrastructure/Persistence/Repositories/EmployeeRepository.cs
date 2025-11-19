using App.Application.Abstractions.Persistence.Repositories;
using App.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Repositories;

public sealed class EmployeeRepository(AppDbContext db) : IEmployeeRepository
{
    public void Add(Employee employee) => db.Employees.Add(employee);
    
    public async Task<Employee?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var employee = await db.Employees
            .AsTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == id, ct);
        
        return employee;
    }
    

}