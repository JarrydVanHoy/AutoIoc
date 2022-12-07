namespace AutoIoc;

public class SingletonServiceAttribute : AutoIocServiceAttribute
{
    public SingletonServiceAttribute() : base(IocLifetime.Singleton)
    {
    }
}