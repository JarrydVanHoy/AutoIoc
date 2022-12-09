using System.Reflection;
using AutoIoc.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Refit;
using ThrowIfArgument;

namespace AutoIoc;

/// <summary>
///     Service collection extensions to add Auto IOC capabilities to your DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    private static bool _init;

    /// <summary>
    ///     Adds AutoIoc to your project which will assembly scan for services, options, and HTTP clients to add to your DI container.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="assembly"></param>
    /// <returns><paramref name="services" /> for chaining</returns>
    public static IServiceCollection AddAutoIoc
    (
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly assembly
    )
    {
        ThrowIf.Argument.IsNull(services);

        if (_init)
        {
            return services;
        }

        _init = true;

        return services.AddAutoIocServices(assembly)
            .AddAutoIocOptions(assembly, configuration)
            .AddAutoIocHttpClients(assembly, configuration);
    }

    private static IServiceCollection AddAutoIocServices
    (
        this IServiceCollection services,
        Assembly assembly
    )
    {
        var iocServices = assembly.GetAutoIocServices().ToList();
        var lifetimeLookup = Enum.GetValues<IocLifetime>().Where(_ => _ is not IocLifetime.None).ToList();

        if (!iocServices.Any())
        {
            return services;
        }

        foreach (var (service, lifetimes) in iocServices)
        {
            foreach (var @interface in service.GetInterfaces()
                         .Where(i => i != typeof(IAsyncDisposable) && i != typeof(IDisposable))
                         .ToList())
            {
                foreach (var lifetime in lifetimeLookup.Where(flag => lifetimes.HasFlag(flag)))
                {
                    switch (lifetime)
                    {
                        case IocLifetime.Transient:
                            services.AddTransient(@interface, service);
                            break;
                        case IocLifetime.Scoped:
                            services.AddScoped(@interface, service);
                            break;
                        case IocLifetime.Singleton:
                            services.AddSingleton(@interface, service);
                            break;
                        case IocLifetime.None:
                        default:
                            throw new ArgumentOutOfRangeException($"Unhandle IOC lifetime: '{lifetime}' for service: '{service.FullName}'");
                    }
                }
            }
        }

        return services;
    }

    private static IServiceCollection AddAutoIocOptions
    (
        this IServiceCollection services,
        Assembly assembly,
        IConfiguration configuration
    )
    {
        var options = assembly.GetAutoIocOptions().ToList();

        if (!options.Any())
        {
            return services;
        }

        services.AddOptions();

        foreach (var (configurationType, configurationSection, required) in options)
        {
            var section = configuration.GetSection(configurationSection);

            if (!section.Exists())
            {
                if (required)
                {
                    throw new AutoIocException($"Cannot find configuration section: '{configurationSection}'");
                }

                Console.WriteLine($"Unable to bind options of type: '{configurationType.Name}' due to missing app settings key: '{configurationSection}'");
                continue;
            }

            services.AddSingleton(
                typeof(IOptionsChangeTokenSource<>).MakeGenericType(configurationType),
                Activator.CreateInstance(typeof(ConfigurationChangeTokenSource<>).MakeGenericType(configurationType), string.Empty, section)
                ?? throw new InvalidOperationException($"Unable to create instance of '{typeof(ConfigurationChangeTokenSource<>)}<{configurationType}>'"));

            services.AddSingleton(
                typeof(IConfigureOptions<>).MakeGenericType(configurationType),
                Activator.CreateInstance(typeof(NamedConfigureFromConfigurationOptions<>).MakeGenericType(configurationType), string.Empty, section, new Action<BinderOptions>(_ => { }))
                ?? throw new InvalidOperationException($"Unable to create instance of '{typeof(NamedConfigureFromConfigurationOptions<>)}<{configurationType}>'"));
        }

        return services;
    }

    private static IServiceCollection AddAutoIocHttpClients
    (
        this IServiceCollection services,
        Assembly assembly,
        IConfiguration configuration
    )
    {
        var addHttpClientMethodInfo = typeof(HttpClientFactoryServiceCollectionExtensions)
            .GetMethods()
            .Single(_ => _.ToString() == "Microsoft.Extensions.DependencyInjection.IHttpClientBuilder AddHttpClient[TClient,TImplementation](Microsoft.Extensions.DependencyInjection.IServiceCollection)");

        var clients = assembly.GetAutoIocHttpClients().ToList();

        if (!clients.Any())
        {
            return services;
        }

        foreach (var (client, primaryHandler, delegatingHandlers, required) in clients)
        {
            IHttpClientBuilder httpClientBuilder;

            if (client.IsInterface)
            {
                if (!client.IsRefitClient())
                {
                    throw new AutoIocException($"Type: '{client.FullName}' does not look to be setup as a Refit client");
                }

                var appSettingsKey = client.Name[1..].Replace("Client", string.Empty);

                if (!configuration.GetSection(appSettingsKey).Exists())
                {
                    if (required)
                    {
                        throw new AutoIocException($"Missing required app settings key: '{appSettingsKey}'.");
                    }

                    Console.WriteLine($"Unable to add the client class type: '{client.Name}' due to missing app settings key: '{appSettingsKey}'");
                    continue;
                }

                var httpClientConfiguration = configuration.GetRequiredConfiguration<HttpClientConfiguration>(appSettingsKey);

                httpClientBuilder = services.AddRefitClient(client)
                    .ConfigureHttpClient(_ =>
                    {
                        _.BaseAddress = httpClientConfiguration.BaseAddress
                                        ?? throw new AutoIocException($"App settings missing value for key '{appSettingsKey}.{nameof(HttpClientConfiguration.BaseAddress)}'.");
                        _.Timeout = TimeSpan.FromSeconds(Math.Abs(httpClientConfiguration.TimeoutSeconds));
                    });
            }
            else
            {
                var @interface = client.GetInterfaces().Single();

                httpClientBuilder = (IHttpClientBuilder) addHttpClientMethodInfo
                    .MakeGenericMethod(@interface, client)
                    .Invoke(null, new object[] {services})!;
            }

            if (primaryHandler is not null)
            {
                httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => (HttpClientHandler) Activator.CreateInstance(primaryHandler)!);
            }

            foreach (var delegatingHandler in delegatingHandlers)
            {
                services.TryAddTransient(delegatingHandler);
                httpClientBuilder.AddHttpMessageHandler(provider => (DelegatingHandler) provider.GetRequiredService(delegatingHandler));
            }
        }

        return services;
    }
}