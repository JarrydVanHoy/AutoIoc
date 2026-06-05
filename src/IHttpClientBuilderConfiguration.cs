using Microsoft.Extensions.DependencyInjection;

namespace AutoIoc;

/// <summary>
///     Implement this interface to apply additional configuration to an
///     <see cref="IHttpClientBuilder" /> after the standard handler pipeline.
/// </summary>
public interface IHttpClientBuilderConfiguration
{
    void Configure(IHttpClientBuilder builder);
}