namespace App.Api.Contracts.Auth.Requests;

public sealed record RegisterRequest(string Email, string Password);