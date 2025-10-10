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
}
