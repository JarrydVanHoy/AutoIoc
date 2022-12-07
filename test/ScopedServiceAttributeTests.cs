using FluentAssertions;
using Xunit;

namespace AutoIoc.UnitTests;

public class ScopedServiceAttributeTests
{
    [Fact]
    public void Ctor_Successful_LifetimesIsOnlyScoped()
    {
        var result = new ScopedServiceAttribute();

        ((int) result.Lifetimes).Should().Be((int) IocLifetime.Scoped);
    }
}