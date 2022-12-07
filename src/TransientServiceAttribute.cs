namespace AutoIoc;

/// <summary>
///     An AutoIocServiceAttribute implementation with a transient lifetime
/// </summary>
public class TransientServiceAttribute : AutoIocServiceAttribute
{
    /// <summary>
    ///     An AutoIocServiceAttribute implementation with a transient lifetime
    /// </summary>
    public TransientServiceAttribute() : base(IocLifetime.Transient)
    {
    }
}