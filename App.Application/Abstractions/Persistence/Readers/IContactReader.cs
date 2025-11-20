using App.Application.Common.Dtos.Contacts;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IContactReader
{
    Task<ContactDetailsDto?> GetDetailsAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<ContactRowDto> items, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? nameFilter,
        bool? ssDeleted,
        CancellationToken ct = default);
}