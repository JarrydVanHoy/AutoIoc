using FluentAssertions;
using Xunit;

namespace AutoIoc.UnitTests;

public class SingletonServiceAttributeTests
{
    [Fact]
    public void Ctor_Successful_LifetimesIsOnlySingleton()
    {
        var result = new SingletonServiceAttribute();

        ((int) result.Lifetimes).Should().Be((int) IocLifetime.Singleton);
    }
}