namespace AutoIoc;

/// <summary>
///     Basic configuration settings for an HTTP client.
/// </summary>
public class HttpClientConfiguration
{
    /// <summary>
    ///     The base address for your HTTP client.
    /// </summary>
    public Uri? BaseAddress { get; set; }

    /// <summary>
    ///     The timeout for your HTTP client.  Defaults to 100 seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 100;
}