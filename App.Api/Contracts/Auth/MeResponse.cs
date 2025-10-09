namespace App.Api.Contracts.Auth;

/// <summary>
/// Represents the authenticated user's identity information extracted from a JWT token.
/// Returned by the <c>/auth/me</c> endpoint to help the frontend confirm and hydrate session state.
/// </summary>
/// <param name="Subject">
/// The unique identifier (JWT <c>sub</c> claim) for the authenticated user. 
/// Typically corresponds to the user's ID in the system.
/// </param>
/// <param name="Email">
/// The user's email address (if available), extracted from the JWT <c>email</c> claim.
/// </param>
public sealed record MeResponse(string Subject, string? Email);