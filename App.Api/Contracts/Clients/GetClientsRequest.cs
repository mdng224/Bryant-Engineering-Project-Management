namespace App.Api.Contracts.Clients;

public record GetClientsRequest(int Page, int PageSize, string? NameFilter);