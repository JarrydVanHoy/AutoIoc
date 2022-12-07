using Microsoft.Extensions.Configuration;
using ThrowIfArgument;

namespace AutoIoc.Extensions;

internal static class ConfigurationExtensions
{
    internal static T GetRequiredConfiguration<T>
    (
        this IConfiguration configuration,
        string key
    )
    {
        ThrowIf.Argument.IsNullOrWhiteSpace(key);

        var section = configuration.GetSection(key);

        if (!section.Exists())
        {
            throw new AutoIocException($"Missing or invalid configuration section: '{key}'");
        }

        var value = (T) section.Get(typeof(T));

        return value is null
            ? throw new AutoIocException($"Missing or invalid configuration section: '{key}'")
            : value;
    }
}