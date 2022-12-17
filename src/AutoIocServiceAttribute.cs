using ThrowIfArgument;

namespace AutoIoc;

/// <summary>
///     The AutoIoc package looks for classes that have this attribute applied and will attempt to add it to your IOC.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AutoIocServiceAttribute : Attribute
{
    internal readonly IocLifetime Lifetimes;

    /// <summary>
    ///     Will add the class to your IOC with the specified <paramref name="lifetimes" />
    /// </summary>
    /// <param name="lifetimes"></param>
    public AutoIocServiceAttribute(
        IocLifetime lifetimes
    )
    {
        Lifetimes = ThrowIf.Argument.IsEqualTo(lifetimes, IocLifetime.None, $"Lifetimes cannot be {IocLifetime.None}");
    }
}

/// <summary>
///     The different lifetime scopes a service can have
/// </summary>
[Flags]
public enum IocLifetime
{
    /// <summary>
    ///     Default value - should never be used
    /// </summary>
    None = 0,
    /// <summary>
    ///     New instance is created upon every injection
    /// </summary>
    Transient = 1,
    /// <summary>
    ///     New instance is create upon every request
    /// </summary>
    Scoped = 1 << 1,
    /// <summary>
    ///     New instance is only created upon first injection
    /// </summary>
    Singleton = 1 << 2
}