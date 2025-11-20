namespace App.Application.Clients.Commands.AddClient;

public sealed record AddClientCommand(
    string Name,
    Guid ClientCategoryId,
    Guid ClientTypeId
);