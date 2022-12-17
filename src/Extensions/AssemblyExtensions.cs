using System.Reflection;

namespace AutoIoc.Extensions;

internal static class AssemblyExtensions
{
    internal static IEnumerable<(Type Service, IocLifetime Lifetimes)> GetAutoIocServices(
        this Assembly assembly
    )
    {
        var attribute = typeof(AutoIocServiceAttribute);

        return assembly
            .GetExportedTypes()
            .Where(type => type.IsClass
                           && type.GetCustomAttributes()
                               .Any(a => a.GetType() == attribute
                                         || a.GetType().IsSubclassOf(attribute)))
            .Select(type => (
                type,
                type.GetCustomAttributes()
                    .Where(a => a.GetType() == attribute || a.GetType().IsSubclassOf(attribute))
                    .Select(a => ((AutoIocServiceAttribute) a).Lifetimes)
                    .Aggregate(
                        (
                            lifetime1,
                            lifetime2
                        ) => lifetime1 | lifetime2)
            ));
    }

    internal static IEnumerable<(Type ConfigurationType, string ConfigurationSection, bool Required)> GetAutoIocOptions(
        this Assembly assembly
    )
    {
        var attribute = typeof(BindOptionsAttribute);

        var types = assembly
            .GetExportedTypes()
            .Where(type => type.IsClass
                           && type.GetCustomAttributes()
                               .Any(a => a.GetType() == attribute));

        var result = types.Select(type => (
                ConfigurationType: type,
                ConfigurationSection: type.GetCustomAttributes()
                    .Where(a => a.GetType() == attribute)
                    .Select(a =>
                    {
                        var configSection = ((BindOptionsAttribute) a).ConfigurationSection;

                        return string.IsNullOrWhiteSpace(configSection)
                            ? type.Name.Replace("Configuration", string.Empty)
                            : configSection;
                    })
                    .First(),
                Required: type.GetCustomAttributes()
                    .Where(a => a.GetType() == attribute)
                    .Select(a => ((BindOptionsAttribute) a).Required)
                    .First()
            ))
            .ToList();

        return result.Select(_ => (_.ConfigurationType, _.ConfigurationSection, _.Required));
    }

    internal static IEnumerable<(Type ClientType, HttpClientAttribute HttpClientAttribute)> GetAutoIocHttpClients(
        this Assembly assembly
    )
    {
        var attribute = typeof(HttpClientAttribute);

        return assembly
            .GetExportedTypes()
            .Where(type =>
                (type.IsClass || type.IsInterface)
                && type.GetCustomAttributes()
                    .Any(a => a.GetType() == attribute))
            .Select(type => (
                type,
                (HttpClientAttribute) type.GetCustomAttributes()
                    .Single(a => a.GetType() == attribute)));
    }
}