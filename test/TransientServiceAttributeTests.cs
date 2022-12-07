using FluentAssertions;
using Xunit;

namespace AutoIoc.UnitTests;

public class TransientServiceAttributeTests
{
    [Fact]
    public void Ctor_Successful_LifetimesIsOnlyTransient()
    {
        var result = new TransientServiceAttribute();

        ((int) result.Lifetimes).Should().Be((int) IocLifetime.Transient);
    }
}