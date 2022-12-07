# AutoIoc

[![NuGet Status](https://img.shields.io/nuget/v/AutoIoc.svg)](https://www.nuget.org/packages/AutoIoc)
[![NuGet](https://img.shields.io/nuget/dt/AutoIoc.svg)](https://www.nuget.org/packages/AutoIoc)

Are you familiar with adding services, options/configurations from app settings, and HTTP client classes to your
dependency injection container? Examples of what I mean:

```C#
services.AddTransient<IFooBarService, FooBarService>();

services.AddOptions<HelloWorldConfiguration>()
    .Configure<IConfiguration>((settings, config) => { config.GetSection("HelloWorld").Bind(settings); });
    
services.AddTransient<AuthDelegatingHandler>();

services.AddHttpClient<IAnimeClient, AnimeClient>()
    .AddHttpMessageHandler<AuthDelegatingHandler>();
    
services.AddRefitClient<IColorClient>()
    .ConfigureHttpClient(c => {...})
    .AddHttpMessageHandler<AuthDelegatingHandler>();
```

Then your code eventually grows to look like:

```C#
services.AddSingleton<IService1, Service1>()
    .AddSingleton<IService2, Service2>()
    .AddSingleton<IService3, Service3>()
    /* pretend there are 100s of these */
    .AddSingleton<IService99, Service99>()
    .AddSingleton<IService100, Service100>();
```

Well this package makes that much easier. It will scan your assembly and fine items that need to be added to your DI
container based on attributes.

## NuGet Installation

Install the [AutoIoc NuGet package](https://nuget.org/packages/AutoIoc):

```.NET CLI
dotnet add package AutoIoc
```

## Usage

Add the following to your `Startup.cs`:

```C#
services.AddAutoIoc(configuration, assembly);
```

### Services

Add services based on the lifetime you'd like to have them have:

```C#
public interface IExample {}
```

- Transient lifetime:

  ```C#
  // always add it to the concrete class as that is where your behavior lives
  [TransientService]
  public class Example : IExample {} 
  
  // this is equivalent to
  services.AddTransient<IExample, Example>();
  ```

- Scoped lifetime:

  ```C#
  // always add it to the concrete class as that is where your behavior lives
  [ScopedService]
  public class Example : IExample {} 
  
  // this is equivalent to
  services.AddScoped<IExample, Example>();
  ```

- Singleton lifetime:

  ```C#
  // always add it to the concrete class as that is where your behavior lives
  [SingletonService]
  public class Example : IExample {} 
  
  // this is equivalent to
  services.AddSingleton<IExample, Example>();
  ```

### Options

Create your POCO class and add the following attribute with the desired app settings key:

```C#
// This will automatically bind the the key `Example`
[BindOptions]
public class ExampleConfiguration
{
    public string Foo { get; set; }
    public string Bar { get; set; }
}

// This will bind the the key `Foo:Bar`
[BindOptions("Foo:Bar")]
public class ExampleConfiguration
{
    public string Foo { get; set; }
    public string Bar { get; set; }
}

// now inject where desired
public class BananaService
{
    private readonly ExampleConfiguration _exampleConfiguration;
    
    public BananaService(IOptions<ExampleConfiguration> exampleConfigurationOptions)
    {
        _exampleConfiguration = exampleConfigurationOptions.Value;
    }
}
```

### HTTP Client Class

#### Typical Route

Normally I see developer creating a client class that injects a name HTTP client from the factory like as follows:

```C#
public interface IDbzClient{}
public class DbzClient : IDbzClient
{
    private readonly HttpClient _httpClient;
    
    public DbzClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}

// add to your DI container
services.AddHttpClient<IDbzClient, DbzClient>();
```

Why not just attribute it?

```C#
public interface IDbzClient {}

// always add it to the implementation since this is where your behavior lives
[HttpClient]
public class DbzClient : IDbzClient {}
```

#### Refit Route

Love using Refit? Me too! Go ahead and add one extra attribute to your interface so you won't need to add another line
to your `Startup.cs`:

```C#
[HttpClient]
public class IDbzClient
{
    [Get("/foo/bar")]
    Task<ApiResponse<string>> GetFooBarAsync();
}

// now inject where desired
public class BananaService
{
    private readonly IDbzClient _dbzClient;
    
    public BananaService(IDbzClient dbzClient)
    {
        _dbzClient = dbzClient;
    }
    
    public async void CallItAsync()
    {
        var result = await _dbzClient.GetFooBarAsync();
        
        if(result.IsSuccessStatusCode)
        {
            Console.WriteLine(result.Content);
        }
    }
}
```

#### Delegating Handlers

Need to add delegating handlers? Well this is how you used to do it:

```C#
public class AuthDelegatingHandler : DelegatingHandler { }

services.AddTransient<AuthDelegatingHandler>();
services.AddHttpClient<IExampleClient, ExampleClient>()
    .AddHttpMessageHandler<AuthDelegatingHandler>();
```

Now you can add as many as you want via attributes:

```C#
public class AuthDelegatingHandler : DelegatingHandler { }

// examples and order matters
[HttpClient(PrimaryHandler = typeof(CustomPrimaryHandler)]
[HttpClient(PrimaryHandler = typeof(CustomPrimaryHandler), typeof(AuthDelegatingHandler)]
[HttpClient(PrimaryHandler = typeof(CustomPrimaryHandler), typeof(AuthDelegatingHandler), typeof(RandomDelegatingHandler)]
[HttpClient(typeof(AuthDelegatingHandler)]  
[HttpClient(typeof(AuthDelegatingHandler), typeof(RandomDelegatingHandler)]  
```

---
Ping me if you want any new features added to the library ❤️
