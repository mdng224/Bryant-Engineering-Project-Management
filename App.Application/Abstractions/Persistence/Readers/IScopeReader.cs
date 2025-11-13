namespace App.Application.Abstractions.Persistence.Readers;

public interface IScopeReader
{
    Task<IReadOnlyDictionary<Guid, string>> GetNamesByIdsAsync(
        IReadOnlyList<Guid> ids,
        CancellationToken ct = default);
}