namespace App.Application.Abstractions.Persistence.Exceptions;

public sealed class UniqueConstraintViolationException(
    string message,
    string? constraintName,
    Exception innerException)
    : Exception(message, innerException)
{
    public string? ConstraintName { get; } = constraintName;
}