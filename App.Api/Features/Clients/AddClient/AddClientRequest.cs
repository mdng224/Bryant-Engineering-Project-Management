using App.Domain.Common;

namespace App.Api.Features.Clients.AddClient;

public sealed record AddClientRequest(
    string Name,
    string? NamePrefix,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? NameSuffix,
    string? Email,
    string? Phone,
    Address? Address,
    string? Note,
    Guid ClientCategoryId,
    Guid ClientTypeId
);