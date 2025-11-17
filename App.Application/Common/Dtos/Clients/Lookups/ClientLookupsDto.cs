namespace App.Application.Common.Dtos.Clients.Lookups;

public sealed record ClientLookupsDto(
    IReadOnlyList<ClientCategoryDto> Categories,
    IReadOnlyList<ClientTypeDto> Types
);
