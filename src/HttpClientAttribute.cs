namespace AutoIoc;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class HttpClientAttribute : Attribute
{
    internal readonly IEnumerable<Type> DelegatingHandlers;
    private Type? _primaryHandler;

    public HttpClientAttribute()
    {
        DelegatingHandlers = Array.Empty<Type>();
    }

    public HttpClientAttribute
    (
        params Type[] delegatingHandlers
    )
    {
        DelegatingHandlers = ValidateDelegatingHandlerTypes(delegatingHandlers);
    }

    public Type? PrimaryHandler
    {
        get => _primaryHandler;
        set => _primaryHandler = ValidatePrimaryHandlerHandlerType(value);
    }

    private static IEnumerable<Type> ValidateDelegatingHandlerTypes
    (
        IEnumerable<Type> delegatingHandlers
    )
    {
        var dhArray = delegatingHandlers as Type[] ?? delegatingHandlers.ToArray();

        var invalid = dhArray.Where(_ => !_.IsSubclassOf(typeof(DelegatingHandler))).ToList();

        if (invalid.Any())
        {
            throw new InvalidOperationException($"HttpClientAttribute received invalid handlers: '{string.Join(", ", invalid)}'");
        }

        var nonDistinct = dhArray.GroupBy(_ => _.FullName).Where(_ => _.Count() > 1).Select(_ => _.Key).ToList();

        if (nonDistinct.Any())
        {
            throw new InvalidOperationException($"HttpClientAttribute received non-distinct handlers: '{string.Join(", ", nonDistinct)}'");
        }

        return dhArray;
    }

    private static Type? ValidatePrimaryHandlerHandlerType
    (
        Type? primaryHandler
    )
    {
        if (primaryHandler is null)
        {
            return null;
        }

        if (!primaryHandler.IsSubclassOf(typeof(HttpClientHandler)))
        {
            throw new ArgumentException($"The primary handler must be of type: '{nameof(HttpClientHandler)}'", nameof(primaryHandler));
        }

        return primaryHandler;
    }
}