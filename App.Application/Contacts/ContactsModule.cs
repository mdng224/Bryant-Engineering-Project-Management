using App.Application.Abstractions.Handlers;
using App.Application.Common.Dtos.Contacts;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Application.Contacts.Queries.GetContactDetails;
using App.Application.Contacts.Queries.ListContacts;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application.Contacts;

public static class ContactsModule
{
    public static IServiceCollection AddContactsApplication(this IServiceCollection services)
    {
        // Queries
        services.AddScoped<IQueryHandler<ListContactsQuery, Result<PagedResult<ContactRowDto>>>, ListContactsHandler>();
        services.AddScoped<IQueryHandler<GetContactDetailsQuery, Result<ContactDetailsDto>>, GetContactDetailsHandler>();
        
        // Commands
        
        return services;
    }
}