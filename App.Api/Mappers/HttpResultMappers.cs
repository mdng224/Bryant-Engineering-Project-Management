using App.Application.Common;

namespace App.Api.Mappers;

public static class HttpResultMappers
{
    public static IResult ToHttpResult(this Result result) =>
        result.IsSuccess
            ? Results.Ok()
            : Results.Problem(result.Error?.Message, statusCode: 400);

    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onOk)
    {
        if (result.IsSuccess) return onOk(result.Value!);

        var error = result.Error;
        if (!error.HasValue)
            return Results.Problem("An unknown error occurred.", statusCode: 400);

        return error.Value.Code switch
        {
            "unauthorized"  => Results.Unauthorized(),
            "conflict"      => Results.Conflict(error.Value.Message),
            _               => Results.Problem(error.Value.Message, statusCode: 400)
        };
    }
}
