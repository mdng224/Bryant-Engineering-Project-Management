using System.Text;

namespace App.Infrastructure.Persistence.Seed.Common;

internal static class SeedResources
{
    public static TextReader OpenText(string resourceName, Encoding? encoding = null)
    {
        var asm = typeof(SeedResources).Assembly;
        var stream = asm.GetManifestResourceStream(resourceName)
                     ?? throw new FileNotFoundException(
                         $"Embedded resource '{resourceName}' not found. " +
                         $"Check Build Action = EmbeddedResource and namespace path.");

        return new StreamReader(stream, encoding ?? Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
    }
}