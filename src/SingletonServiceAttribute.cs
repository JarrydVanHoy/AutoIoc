namespace AutoIoc;

/// <summary>
///     An AutoIocServiceAttribute implementation with a singleton lifetime
/// </summary>
public class SingletonServiceAttribute : AutoIocServiceAttribute
{
    /// <summary>
    ///     An AutoIocServiceAttribute implementation with a singleton lifetime
    /// </summary>
    public SingletonServiceAttribute() : base(IocLifetime.Singleton)
    {
    }
}