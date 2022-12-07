using System.Reflection;

namespace AutoIoc.Extensions;

internal static class TypeExtensions
{
    internal static bool IsRefitClient
    (
        this Type type
    )
    {
        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .ToList();

        return methods.Any(m => m.GetCustomAttributes()
            .Any(a => a.GetType().Namespace?.StartsWith("Refit") ?? false));
    }
}