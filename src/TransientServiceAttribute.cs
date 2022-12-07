namespace AutoIoc;

public class TransientServiceAttribute : AutoIocServiceAttribute
{
    public TransientServiceAttribute() : base(IocLifetime.Transient)
    {
    }
}