using App.Api.Features.Contacts.GetContactDetails;
using App.Api.Features.Contacts.ListContacts;

namespace App.Api.Features.Contacts;

public static class ContactModule
{
    public static void MapContactEndpoints(this IEndpointRouteBuilder app)
    {
        var contacts = app.MapGroup("/contacts")
            .WithTags("Contacts");

        contacts.MapGetContactDetailsEndpoint();
        contacts.MapListContactsEndpoint();
    }
}