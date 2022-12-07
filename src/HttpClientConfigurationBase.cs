namespace AutoIoc;

public class HttpClientConfigurationBase
{
    public Uri? BaseAddress { get; set; }

    public int TimeoutSeconds { get; set; } = 100;
}