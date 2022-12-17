using Microsoft.Extensions.Configuration;
using ThrowIfArgument;

namespace AutoIoc.Extensions;

internal static class ConfigurationExtensions
{
    internal static T GetRequiredConfiguration<T>(
        this IConfiguration configuration,
        string appSettingsKey
    )
    {
        ThrowIf.Argument.IsNullOrWhiteSpace(appSettingsKey);

        var section = configuration.GetSection(appSettingsKey);

        if (!section.Exists())
        {
            throw new AutoIocException($"Missing or invalid configuration section: '{appSettingsKey}'");
        }

        var value = (T?) section.Get(typeof(T?));

        Console.WriteLine(section.Value);

        return value is null
            ? throw new AutoIocException($"Missing or invalid configuration section: '{appSettingsKey}'")
            : value;
    }
}