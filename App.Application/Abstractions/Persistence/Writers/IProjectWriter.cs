using App.Domain.Projects;

namespace App.Application.Abstractions.Persistence.Writers;

public interface IProjectWriter
{
    void Add(Project project);
    Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default);
    void Update(Project project);
}