namespace App.Api.Contracts.Clients.Requests;

public record GetClientsRequest(int Page, int PageSize, string? NameFilter, bool? IsDeleted);