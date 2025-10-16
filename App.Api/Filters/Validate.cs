using FluentValidation;

namespace App.Api.Filters;

/// <summary>
/// Generic endpoint filter that runs FluentValidation before invoking the handler.
/// Returns a 400 ValidationProblem result if validation fails.
/// </summary>
public sealed class Validate<T>(IValidator<T> validator) : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
    {
        var model = context.Arguments.OfType<T>().FirstOrDefault();
        if (model is null)
            return TypedResults.BadRequest("Invalid request payload."); // (optional) typed for consistency

        var result = await validator.ValidateAsync(model, context.HttpContext.RequestAborted);
        if (!result.IsValid)
            return TypedResults.ValidationProblem(result.ToDictionary());

        return await next(context);
    }
}