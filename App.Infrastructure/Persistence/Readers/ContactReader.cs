using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos.Contacts;
using App.Domain.Contacts;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Readers;

public sealed class ContactReader(AppDbContext db) : IContactReader
{
    public async Task<ContactDetailsDto?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        var contactDetails = await db.ReadSet<Contact>()
            .Where(p => p.Id == id)
            .Select(p => new ContactDetailsDto(
                p.Id,
                p.ClientId,
                p.NamePrefix,
                p.FirstName,
                p.MiddleName,
                p.LastName,
                p.NameSuffix,
                p.Company,
                p.Department,
                p.JobTitle,
                p.Address != null ? p.Address.Line1      : null,
                p.Address != null ? p.Address.Line2      : null,
                p.Address != null ? p.Address.City       : null,
                p.Address != null ? p.Address.State      : null,
                p.Address != null ? p.Address.PostalCode : null,
                p.Country,
                p.BusinessPhone,
                p.MobilePhone,
                p.PrimaryPhone,
                p.Email,
                p.WebPage,
                p.IsPrimaryForClient,
                p.CreatedAtUtc,
                p.UpdatedAtUtc,
                p.DeletedAtUtc,
                p.CreatedById,
                p.UpdatedById,
                p.DeletedById
            ))
            .SingleOrDefaultAsync(ct);

        return contactDetails;
    }

    public async Task<(IReadOnlyList<ContactRowDto> items, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? nameFilter,
        bool? isDeleted,
        CancellationToken ct = default)
    {
        var query = BuildContactQuery(nameFilter, isDeleted);

        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0 || skip >= totalCount)
            return ([], totalCount);

        var items = await query
            .Skip(skip)
            .Take(take)
            .Select(p => new ContactRowDto(
                p.Id,
                p.ClientId,
                p.FirstName,
                p.LastName,
                p.Company,
                p.JobTitle,
                p.Email,
                p.BusinessPhone,
                p.IsPrimaryForClient,
                p.DeletedAtUtc
            ))
            .ToListAsync(ct);

        return (items, totalCount);
    }

    private IQueryable<Contact> BuildContactQuery(string? nameFilter, bool? isDeleted)
    {
        var query = db.ReadSet<Contact>().ApplyDeletedFilter(isDeleted);

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            var pattern = $"%{nameFilter.Trim()}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.FirstName, pattern) ||
                EF.Functions.ILike(p.LastName, pattern));
        }

        return query
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ThenBy(p => p.Id);
    }
}