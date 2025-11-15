namespace App.Application.Abstractions.Persistence.Exceptions;

public sealed class UniqueConstraintViolationException(string message, Exception inner) : Exception(message, inner);
