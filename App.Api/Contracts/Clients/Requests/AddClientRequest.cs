using App.Domain.Common;

namespace App.Api.Contracts.Clients.Requests;

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
    Guid ClientTypeId);