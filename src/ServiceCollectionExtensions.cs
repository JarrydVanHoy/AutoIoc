using AutoIoc.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Refit;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using ThrowIfArgument;

namespace AutoIoc;

/// <summary>
///     Service collection extensions to add Auto IOC capabilities to your DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    private static readonly HashSet<Assembly> Init = new();
    private static JsonSerializerOptions? _defaultSerializerOptions;

    /// <summary>
    ///     Adds AutoIoc to your project which will assembly scan for services, options, and HTTP clients to add to your DI container.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="assembly"></param>
    /// <returns><paramref name="services" /> for chaining</returns>
    public static IServiceCollection AddAutoIoc(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly assembly
    )
    {
        return services.AddAutoIoc(configuration, new[] {assembly});
    }

    /// <summary>
    ///     Adds AutoIoc to your project which will assembly scan for services, options, and HTTP clients to add to your DI container.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="assemblies"></param>
    /// <returns><paramref name="services" /> for chaining</returns>
    public static IServiceCollection AddAutoIoc(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies
    )
    {
        ThrowIf.Argument.IsNull(services);
        ThrowIf.Argument.IsNull(configuration);
        ThrowIf.Argument.IsEmpty(assemblies);

        assemblies.ToList().ForEach(assembly =>
        {
            if (Init.Contains(assembly))
            {
                return;
            }

            Init.Add(assembly);

            services
                .AddAutoIocServices(assembly)
                .AddAutoIocOptions(assembly, configuration)
                .AddAutoIocHttpClients(assembly, configuration);
        });

        return services;
    }

    private static IServiceCollection AddAutoIocServices(
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

    private static IServiceCollection AddAutoIocOptions(
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

    private static IServiceCollection AddAutoIocHttpClients(
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

        foreach (var (client, httpClientAttribute) in clients)
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
                    if (httpClientAttribute.Required)
                    {
                        throw new AutoIocException($"Missing required app settings key: '{appSettingsKey}'.");
                    }

                    Console.WriteLine($"Unable to add the client class type: '{client.Name}' due to missing app settings key: '{appSettingsKey}'");
                    continue;
                }

                var httpClientConfiguration = configuration.GetRequiredConfiguration<HttpClientConfiguration>(appSettingsKey);

                httpClientBuilder = services
                    .AddRefitClient(
                        client,
                        new RefitSettings(new SystemTextJsonContentSerializer(httpClientAttribute.JsonSerializerOptions ?? GetDefaultSerializerOptions())))
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

            if (httpClientAttribute.PrimaryHandler is not null)
            {
                httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => (HttpClientHandler) Activator.CreateInstance(httpClientAttribute.PrimaryHandler)!);
            }

            foreach (var delegatingHandler in httpClientAttribute.DelegatingHandlers)
            {
                services.TryAddTransient(delegatingHandler);
                httpClientBuilder.AddHttpMessageHandler(provider => (DelegatingHandler) provider.GetRequiredService(delegatingHandler));
            }
        }

        return services;
    }

    private static JsonSerializerOptions GetDefaultSerializerOptions()
    {
        if (_defaultSerializerOptions is not null)
        {
            return _defaultSerializerOptions;
        }

        _defaultSerializerOptions = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _defaultSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        _defaultSerializerOptions.Converters.Add(new JsonStringDecimalConverter());

        return _defaultSerializerOptions;
    }
}

internal class JsonStringDecimalConverter : JsonConverter<decimal>
{
    public override decimal Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        decimal ParseString(
            string? value
        )
        {
            return string.IsNullOrWhiteSpace(value)
                ? default
                : decimal.Parse(value.Replace(",", string.Empty));
        }

        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetDecimal(),
            JsonTokenType.String => ParseString(reader.GetString()),
            JsonTokenType.None
                or JsonTokenType.StartObject
                or JsonTokenType.EndObject
                or JsonTokenType.StartArray
                or JsonTokenType.EndArray
                or JsonTokenType.PropertyName
                or JsonTokenType.Comment
                or JsonTokenType.True
                or JsonTokenType.False
                or JsonTokenType.Null
                or _ => default
        };
    }

    public override void Write(
        Utf8JsonWriter writer,
        decimal value,
        JsonSerializerOptions options
    )
    {
        if (options.NumberHandling == JsonNumberHandling.WriteAsString)
        {
            writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
        }
        else
        {
            writer.WriteNumberValue(value);
        }
    }
}