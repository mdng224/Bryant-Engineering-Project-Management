using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace App.Infrastructure.Persistence;

public sealed class EfUnitOfWork(AppDbContext db) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
                                           {
                                               SqlState: PostgresErrorCodes.UniqueViolation
                                           } pg)
        {
            throw new UniqueConstraintViolationException(
                message: "Unique constraint violation.",
                constraintName: pg.ConstraintName,
                innerException: ex
            );
        }
    }
}