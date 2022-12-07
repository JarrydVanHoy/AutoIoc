namespace AutoIoc;

public class ScopedServiceAttribute : AutoIocServiceAttribute
{
    public ScopedServiceAttribute() : base(IocLifetime.Scoped)
    {
    }
}