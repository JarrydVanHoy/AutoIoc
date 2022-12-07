using ThrowIfArgument;

namespace AutoIoc;

[AttributeUsage(AttributeTargets.Class)]
public class AutoIocServiceAttribute : Attribute
{
    internal readonly IocLifetime Lifetimes;

    protected AutoIocServiceAttribute
    (
        IocLifetime lifetimes
    )
    {
        Lifetimes = ThrowIf.Argument.IsEqualTo(lifetimes, IocLifetime.None, $"Lifetimes cannot be {IocLifetime.None}");
    }
}

[Flags]
public enum IocLifetime
{
    None = 0,
    Transient = 1,
    Scoped = 1 << 1,
    Singleton = 1 << 2
}