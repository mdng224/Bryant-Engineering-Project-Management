using App.Domain.Common;

namespace App.Application.Clients.Commands.AddClient;

public sealed record AddClientCommand(
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