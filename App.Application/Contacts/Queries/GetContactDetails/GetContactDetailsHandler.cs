using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos.Contacts;
using App.Application.Common.Results;
using static App.Application.Common.R;

namespace App.Application.Contacts.Queries.GetContactDetails;

public class GetContactDetailsHandler(IContactReader reader)
    : IQueryHandler<GetContactDetailsQuery, Result<ContactDetailsDto>>
{
    public async Task<Result<ContactDetailsDto>> Handle(GetContactDetailsQuery query, CancellationToken ct)
    {
        var dto = await reader.GetDetailsAsync(query.Id, ct);

        return dto is null ? Fail<ContactDetailsDto>("not_found", "Contact not found.") : Ok(dto);
    }
}