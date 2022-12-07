using System;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using Xunit;

namespace AutoIoc.UnitTests;

public class HttpClientAttributeTests
{
    [Fact]
    public void Ctor_NoHandlersPassedIn_DelegatingHandlersIsEmpty()
    {
        var result = new HttpClientAttribute();

        result.DelegatingHandlers.Should().BeEmpty();
    }

    [Fact]
    public void Ctor_NoPrimaryHandlerSet_PrimaryHandlerDefaultNull()
    {
        var result = new HttpClientAttribute();

        result.PrimaryHandler.Should().BeNull();
    }

    [Fact]
    public void PrimaryHandlerSetter_Null_StaysNull()
    {
        var sut = new HttpClientAttribute
        {
            PrimaryHandler = null
        };

        sut.PrimaryHandler.Should().BeNull();
    }

    [Fact]
    public void PrimaryHandlerSetter_Invalid_ThrowsArgumentException()
    {
        var sut = new HttpClientAttribute();

        var result = Record.Exception(() => sut.PrimaryHandler = typeof(HttpClientAttributeTests));

        result.Should().BeOfType<ArgumentException>();
        result!.Message.Should().StartWith($"The primary handler must be of type: '{nameof(HttpClientHandler)}'");
    }

    [Fact]
    public void PrimaryHandlerSetter_Valid_ReturnsExpected()
    {
        var sut = new HttpClientAttribute
        {
            PrimaryHandler = typeof(TestPrimaryHandler)
        };

        sut.PrimaryHandler.Should().Be(typeof(TestPrimaryHandler));
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(int))]
    [InlineData(typeof(bool))]
    [InlineData(typeof(object))]
    public void Ctor_NonDelegatingHandlerType_ThrowsInvalidOperationException
    (
        Type delegatingHandler
    )
    {
        var result = Record.Exception(() => new HttpClientAttribute(delegatingHandler));

        result.Should().BeOfType<InvalidOperationException>();
        result!.Message.Should().StartWith("HttpClientAttribute received invalid handlers:");
    }

    [Theory]
    [InlineData(typeof(Test1DelegatingHandler))]
    [InlineData(typeof(Test2DelegatingHandler))]
    public void Ctor_PassedSingleHandler_DelegatingHandlersIsExpected
    (
        Type delegatingHandler
    )
    {
        var result = new HttpClientAttribute(delegatingHandler);

        result.DelegatingHandlers.Should().HaveCount(1);
        result.DelegatingHandlers.Should().Contain(delegatingHandler);
    }

    [Fact]
    public void Ctor_MultipleOfSameHandlerPassedIn_ThrowsInvalidOperationException()
    {
        var result = Record.Exception(() => new HttpClientAttribute(typeof(Test1DelegatingHandler), typeof(Test1DelegatingHandler)));

        result.Should().BeOfType<InvalidOperationException>();
        result!.Message.Should().StartWith("HttpClientAttribute received non-distinct handlers:");
    }

    [Fact]
    public void Ctor_MultipleHandlersPassedIn_DelegatingHandlersIsExpected()
    {
        var result = new HttpClientAttribute(typeof(Test1DelegatingHandler), typeof(Test2DelegatingHandler));

        result.DelegatingHandlers.Should().HaveCount(2);
        result.DelegatingHandlers.Should().Contain(typeof(Test1DelegatingHandler));
        result.DelegatingHandlers.Should().Contain(typeof(Test2DelegatingHandler));
    }

    [Theory]
    [InlineData(typeof(Test1DelegatingHandler), typeof(Test2DelegatingHandler))]
    [InlineData(typeof(Test2DelegatingHandler), typeof(Test1DelegatingHandler))]
    public void Ctor_MultipleHandlersPassedIn_DelegatingHandlersOrderRetained
    (
        Type delegatingHandler1,
        Type delegatingHandler2
    )
    {
        var result = new HttpClientAttribute(delegatingHandler1, delegatingHandler2);

        result.DelegatingHandlers.Should().HaveCount(2);
        result.DelegatingHandlers.First().Should().Be(delegatingHandler1);
        result.DelegatingHandlers.Last().Should().Be(delegatingHandler2);
    }

    private class Test1DelegatingHandler : DelegatingHandler
    {
    }

    private class Test2DelegatingHandler : DelegatingHandler
    {
    }

    private class TestPrimaryHandler : HttpClientHandler
    {
    }
}