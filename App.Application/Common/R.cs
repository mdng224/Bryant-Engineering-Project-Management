namespace App.Application.Common;

public static class R
{
    public static Result<T> Ok<T>(T value) => Result<T>.Success(value);
    public static Result<T> Fail<T>(string code, string message) => Result<T>.Fail(code, message);
}
