using App.Application.Common.Results;

namespace App.Api.Common.Mappers;

internal static class HttpResultMappers
{
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onOk)
        => result.IsSuccess ? onOk(result.Value!) : MapError(result.Error);

    private static IResult MapError(Error? error)
    {
        if (error is null)
            return Results.Problem("An unknown error occurred.", statusCode: StatusCodes.Status400BadRequest);

        var e = error.Value;

        // convention: "<category>.<field>" e.g., "conflict.email", "validation.password"
        var parts = e.Code.Split('.', 2, StringSplitOptions.TrimEntries);
        var category = parts[0];                 // "conflict"
        var field = parts.Length > 1 ? parts[1] : null; // "email" or null

        return category switch
        {
            "unauthorized" => Results.Problem(
                title: "Unauthorized",
                detail: e.Message,
                statusCode: StatusCodes.Status401Unauthorized,
                extensions: new Dictionary<string, object?> { ["code"] = e.Code }),

            "conflict" => Results.Problem(
                title: "Conflict",
                detail: e.Message,
                statusCode: StatusCodes.Status409Conflict,
                extensions: BuildExtensions(e.Code, field, e.Message)),

            // You can add "validation" here if you batch multiple field errors
            _ => Results.Problem(
                detail: e.Message,
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?> { ["code"] = e.Code })
        };
    }

    private static Dictionary<string, object?> BuildExtensions(string code, string? field, string message)
    {
        var ext = new Dictionary<string, object?> { ["code"] = code };

        if (!string.IsNullOrWhiteSpace(field))
        {
            ext["errors"] = new Dictionary<string, string[]>
            {
                [field] = [message]
            };
        }

        return ext;
    }
}
