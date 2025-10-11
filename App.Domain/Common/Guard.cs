namespace App.Domain.Common;

public static class Guard
{
    public static string AgainstNullOrWhiteSpace(string? input, string paramName)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException($"{paramName} cannot be null or empty.", paramName);

        return input;
    }

    public static T AgainstNull<T>(T? input, string paramName) where T : class
    {
        if (input is null) throw new ArgumentNullException(paramName);

        return input;
    }

    public static void AgainstInvalid<T>(T input, Func<T, bool> predicate, string message, string paramName)
    {
        if (!predicate(input)) throw new ArgumentException(message, paramName);
    }

    // default-value guard (non-nullable)
    public static T AgainstDefault<T>(T input, string paramName)
        where T : struct, IEquatable<T>
    {
        if (input.Equals(default))
            throw new ArgumentException($"{paramName} cannot be the default value.", paramName);
        return input;
    }

    // default-value guard (nullable)
    public static T AgainstDefault<T>(T? input, string paramName)
        where T : struct, IEquatable<T>
    {
        if (!input.HasValue || input.Value.Equals(default))
            throw new ArgumentException($"{paramName} cannot be null or the default value.", paramName);
        return input.Value;
    }
}
