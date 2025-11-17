namespace App.Application.Common.Dtos.Clients.Lookups;

public sealed record ClientTypeDto(Guid Id, string Name, string Description, Guid CategoryId);