using System.Collections.Generic;
using System.Reflection;
using AutoIoc.UnitTests.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AutoIoc.UnitTests;

public class ServiceCollectionExtensionsTests
{
    private readonly Assembly _assembly = Assembly.GetExecutingAssembly();
    private readonly IConfiguration _configuration;
    private readonly IServiceCollection _sut = new ServiceCollection();

    public ServiceCollectionExtensionsTests()
    {
        _configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                {nameof(AssemblyExtensionsTests.TestAutoIocConfig), string.Empty},
                {$"{nameof(TypeExtensionsTests.IRefitInterface)[1..]}:{nameof(HttpClientConfiguration.BaseAddress)}", "http://fake.domain.com"}
            })
            .Build();
    }

    [Fact]
    public void AddAutoIoc_ReturnsSuccessfully()
    {
        var result = Record.Exception(() => _sut.AddAutoIoc(_configuration, _assembly));

        result.Should().BeNull();
    }

    [Fact]
    public void AddAutoIoc_CalledTwice_ReturnsSuccessfully()
    {
        var result = Record.Exception(() => _sut.AddAutoIoc(_configuration, _assembly).AddAutoIoc(_configuration, _assembly));

        result.Should().BeNull();
    }
}