namespace App.Application.Common.Results;

/// <summary>
/// Represents a type with a single possible value, used when a method or command
/// conceptually returns "nothing" but a generic type argument is still required.
/// </summary>
/// <remarks>
/// <para>
/// In C#, <see cref="void"/> cannot be used as a generic type parameter (e.g., you can’t write <c>Result&lt;void&gt;</c>).
/// The <see cref="Unit"/> type solves that limitation by providing a placeholder value
/// that represents "no result".
/// </para>
/// <para>
/// Commonly used in CQRS command handlers where an operation has side effects
/// (e.g., create, update, delete) but doesn’t return a data payload.
/// </para>
/// </remarks>
public readonly struct Unit
{
    public static readonly Unit Value = new();
}