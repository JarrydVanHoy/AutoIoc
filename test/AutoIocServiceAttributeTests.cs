using System;
using System.ComponentModel.DataAnnotations;
using AutoFixture.Xunit2;
using FluentAssertions;
using Xunit;

namespace AutoIoc.UnitTests;

public class AutoIocServiceAttributeTests
{
    [Fact]
    public void Ctor_PassedLifetimeNone_ThrowsArgumentException()
    {
        var result = Record.Exception(() => new AutoIocServiceAttribute(IocLifetime.None));

        result.Should().BeOfType<ArgumentException>();
        result!.Message.Should().StartWith($"Lifetimes cannot be {IocLifetime.None}");
    }

    [Theory]
    [AutoData]
    public void Ctor_PassedLifetimes_LifetimesIsExpected
    (
        [Range(1, 4)] int iocLifetimes
    )
    {
        var result = new AutoIocServiceAttribute((IocLifetime) iocLifetimes);

        ((int) result.Lifetimes).Should().Be(iocLifetimes);
    }
}