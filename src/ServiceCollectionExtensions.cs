using System.Reflection;
using AutoIoc.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Refit;
using ThrowIfArgument;

namespace AutoIoc;

public static class ServiceCollectionExtensions
{
    private static bool _init;

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

        foreach (var (configurationType, configurationSection) in options)
        {
            var section = configuration.GetSection(configurationSection);

            if (!section.Exists())
            {
                throw new AutoIocException($"Cannot find configuration section: '{configurationSection}'");
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
            .Single(_ => _.ToString() ==
                         "Microsoft.Extensions.DependencyInjection.IHttpClientBuilder AddHttpClient[TClient,TImplementation](Microsoft.Extensions.DependencyInjection.IServiceCollection)");

        var clients = assembly.GetAutoIocHttpClients().ToList();

        if (!clients.Any())
        {
            return services;
        }

        foreach (var (service, primaryHandler, delegatingHandlers) in clients)
        {
            IHttpClientBuilder httpClientBuilder;

            if (service.IsInterface)
            {
                var appSettingsKey = service.Name[1..].Replace("Client", string.Empty);

                if (!configuration.GetSection(appSettingsKey).Exists())
                {
                    throw new AutoIocException($"Missing required app settings key: '{appSettingsKey}'.");
                }

                var httpClientConfiguration = configuration.GetRequiredConfiguration<HttpClientConfigurationBase>(appSettingsKey);

                httpClientBuilder = services.AddRefitClient(service)
                    .ConfigureHttpClient(_ =>
                    {
                        _.BaseAddress = httpClientConfiguration.BaseAddress
                                        ?? throw new AutoIocException($"App settings missing value for key '{appSettingsKey}.{nameof(HttpClientConfigurationBase.BaseAddress)}'.");
                        _.Timeout = TimeSpan.FromSeconds(Math.Abs(httpClientConfiguration.TimeoutSeconds));
                    });
            }
            else
            {
                var @interface = service.GetInterfaces().Single();

                httpClientBuilder = (IHttpClientBuilder) addHttpClientMethodInfo
                    .MakeGenericMethod(@interface, service)
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