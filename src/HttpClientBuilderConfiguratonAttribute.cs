using ThrowIfArgument;

namespace AutoIoc;

/// <summary>
///     Apply additional configuration to an <see cref="IHttpClientBuilder" />
///     after the handler pipeline declared by <see cref="HttpClientAttribute" />
///     has been registered.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class HttpClientBuilderConfiguratonAttribute(Type configuratorType) : Attribute
{
    internal readonly Type ConfiguratorType = ValidateConfiguratorType(configuratorType);

    private static Type ValidateConfiguratorType(Type configuratorType)
    {
        ThrowIf.Argument.IsNull(configuratorType);
    
        if (!typeof(IHttpClientBuilderConfiguration).IsAssignableFrom(configuratorType))
        {
            throw new ArgumentException(
                $"The configurator must implement '{nameof(IHttpClientBuilderConfiguration)}'",
                nameof(configuratorType));
        }
        if (configuratorType.IsAbstract || configuratorType.IsInterface)
        {
            throw new ArgumentException(
                "The configurator type must be a concrete class",
                nameof(configuratorType));
        }

        return configuratorType;
    }
}