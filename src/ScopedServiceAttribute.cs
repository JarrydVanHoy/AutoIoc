namespace AutoIoc;

/// <summary>
///     An AutoIocServiceAttribute implementation with a scoped lifetime
/// </summary>
public class ScopedServiceAttribute : AutoIocServiceAttribute
{
    /// <summary>
    ///     An AutoIocServiceAttribute implementation with a scoped lifetime
    /// </summary>
    public ScopedServiceAttribute() : base(IocLifetime.Scoped)
    {
    }
}