namespace App.Application.Common.Results;

/// <summary>Represents an error with a unique code and a message.</summary>
public readonly record struct Error(string Code, string Message);