using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Exceptions;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Abstractions.Persistence.Repositories;
using App.Application.Clients.Mappers;
using App.Application.Common.Results;
using App.Domain.Common;
using static App.Application.Common.R;

namespace App.Application.Clients.Commands.AddClient;

public class AddClientHandler(IClientReader reader, IClientRepository repository, IUnitOfWork uow)
    : ICommandHandler<AddClientCommand, Result<Guid>>
{
    private const string EmailConflictCode = "conflict";
    private const string EmailConflictMessage = "A client with this email already exists.";

    public async Task<Result<Guid>> Handle(AddClientCommand command, CancellationToken ct)
    {
        var normalizedEmail = command.Email?.ToNormalizedEmail();
        var normalizedPhone = command.Phone?.ToNormalizedPhone();
        if (string.IsNullOrWhiteSpace(normalizedEmail) && string.IsNullOrWhiteSpace(normalizedPhone))
            return Fail<Guid>(code: "validation", message: "At least one of Email or Phone is required.");

        if (!string.IsNullOrWhiteSpace(normalizedEmail) && await reader.EmailExistsAsync(normalizedEmail, ct))
            return Fail<Guid>(EmailConflictCode, EmailConflictMessage);
        
        var client = command.ToDomain();
        
        try
        {
            repository.Add(client);
            await uow.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintViolationException)
        {
            return Fail<Guid>(EmailConflictCode, EmailConflictMessage);
        }
        
        return Ok(client.Id);
    }
}