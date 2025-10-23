using System.Security.Cryptography;
using System.Text;
using App.Application.Abstractions;
using App.Application.Auth.Commands.VerifyEmail;
using App.Application.Common;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Auth;

public static class VerificationEndpoints
{
    public static void MapVerificationEndpoints(this IEndpointRouteBuilder app)
    {
        var verify = app.MapGroup("/auth/verify")
            .AllowAnonymous()
            .WithTags("Verification")
            .WithOpenApi();

        // GET /auth/verify?token=...
        verify.MapGet("/", HandleVerificationAsync)
            .WithSummary("Verify email")
            .WithDescription("Consumes a one-time token and activates the user.")
            .Produces(302) // Redirect to frontend result page
            .ProducesProblem(400)
            .ProducesProblem(409)
            .ProducesProblem(410)
            .ProducesProblem(500);
    }

    private static async Task<IResult> HandleVerificationAsync(
        [FromQuery] string token,
        [FromServices] ICommandHandler<VerifyEmailCommand, Result<VerifyEmailResult>> handler,
        [FromServices] IConfiguration cfg,
        CancellationToken ct)
    {
        var command =  new VerifyEmailCommand(token);
        var result = await handler.Handle(command, ct);
        var status = result.Value!.Outcome.ToName();
        var message = result.Value!.Outcome.ToMessage();
        var statusCode = result.Error?.Code is { } code ? code.ToStatusCode() : 500;

        if (result.IsSuccess)
            return Redirect(GetFrontendUrl(cfg, status, message));
        
        var title = result.Value?.Outcome.ToMessage() ?? result.Error?.Message ?? "Unexpected error.";
        
        return Problem(title: title, statusCode: statusCode);
    }
    
    private static string GetFrontendUrl(IConfiguration cfg, string status, string message)
    {
        var baseUrl = (cfg["Frontend:BaseUrl"] ?? "/").TrimEnd('/');
        return $"{baseUrl}/verify-result?status={status}&message={Uri.EscapeDataString(message)}";
    }

}