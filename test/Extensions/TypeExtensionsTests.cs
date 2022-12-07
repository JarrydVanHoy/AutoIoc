using System;
using System.Threading.Tasks;
using AutoIoc.Extensions;
using FluentAssertions;
using Refit;
using Xunit;

namespace AutoIoc.UnitTests.Extensions;

public class TypeExtensionsTests
{
    [Theory]
    [InlineData(typeof(INonRefitInterface))]
    [InlineData(typeof(NonRefitConcrete))]
    public void IsRefitClient_NotRefitType_ReturnsFalse
    (
        Type type
    )
    {
        var result = type.IsRefitClient();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsRefitClient_IsRefitType_ReturnsTrue()
    {
        var type = typeof(IRefitInterface);

        var result = type.IsRefitClient();

        result.Should().BeTrue();
    }

    public interface INonRefitInterface
    {
    }

    [HttpClient]
    public class NonRefitConcrete : INonRefitInterface
    {
    }

    [HttpClient]
    public interface IRefitInterface
    {
        [Get("/foo/bar")]
        Task<ApiResponse<string>> GetFooBarAsync();
    }
}