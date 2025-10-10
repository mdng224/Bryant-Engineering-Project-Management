namespace App.Application.Common;

/// <summary> Represents the outcome of an operation that does not return a value.</summary>
public sealed class Result
{
    public bool IsSuccess { get; }
    public Error? Error { get; }

    private Result(bool ok, Error? error) => (IsSuccess, Error) = (ok, error);
    public static Result Success() => new(true, null);
    public static Result Fail(string code, string message) => new(false, new Error(code, message));
}

/// <summary>
/// Represents the outcome of an operation that returns a value of type <typeparamref name="T"/>.
/// </summary>
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }

    private Result(bool isSuccess, T? value, Error? error)
        => (IsSuccess, Value, Error) = (isSuccess, value, error);

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Fail(string code, string message) => new(false, default, new Error(code, message));
}
