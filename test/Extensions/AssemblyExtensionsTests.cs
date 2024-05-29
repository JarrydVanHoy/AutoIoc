using AutoIoc.Extensions;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace AutoIoc.UnitTests.Extensions;

public class AssemblyExtensionsTests
{
    private readonly Assembly _sut = Assembly.GetExecutingAssembly();

    [Fact]
    public void GetAutoIocServices_ReturnsExpected()
    {
        var result = _sut.GetAutoIocServices();

        result.Should().Contain((typeof(TestAutoIocService), IocLifetime.Transient | IocLifetime.Scoped | IocLifetime.Singleton));
    }

    [Fact]
    public void GetAutoIocOptions_ReturnsExpected()
    {
        var result = _sut.GetAutoIocOptions();

        result.Should().Contain((typeof(TestAutoIocConfig), nameof(TestAutoIocConfig), true));
    }

    [Fact]
    public void GetAutoIocHttpClients_ReturnsExpected()
    {
        var result = _sut.GetAutoIocHttpClients().ToList();

        result.Select(_ => _.ClientType).Should().Contain(typeof(TestAutoIocClient));
        result.First(_ => _.ClientType == typeof(TestAutoIocClient)).HttpClientAttribute.DelegatingHandlers.Should().BeEmpty();
    }

    public interface ITestAutoIocService
    {
    }

    [AutoIocService(IocLifetime.Transient | IocLifetime.Singleton)]
    [ScopedService]
    public class TestAutoIocService : ITestAutoIocService
    {
    }

    [BindOptions(nameof(TestAutoIocConfig))]
    public class TestAutoIocConfig
    {
    }

    [BindOptions(nameof(TestAutoIocConfig))]
    public class TestChildAutoIocConfig : TestAutoIocClient
    {
        public List<string> Type { get; set; }
    }

    public interface ITestAutoIocClient
    {
    }

    [HttpClient]
    public class TestAutoIocClient : ITestAutoIocClient
    {
    }
}