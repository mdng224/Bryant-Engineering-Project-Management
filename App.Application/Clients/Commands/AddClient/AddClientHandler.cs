using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Exceptions;
using App.Application.Abstractions.Persistence.Repositories;
using App.Application.Clients.Mappers;
using App.Application.Common.Results;
using static App.Application.Common.R;

namespace App.Application.Clients.Commands.AddClient;

public class AddClientHandler(IClientRepository repository, IUnitOfWork uow)
    : ICommandHandler<AddClientCommand, Result<Guid>>
{
    private const string EmailConflictCode = "conflict";
    private const string EmailConflictMessage = "A client with this email already exists.";

    public async Task<Result<Guid>> Handle(AddClientCommand command, CancellationToken ct)
    {
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