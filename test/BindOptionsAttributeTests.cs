using AutoFixture.Xunit2;
using FluentAssertions;
using Xunit;

namespace AutoIoc.UnitTests;

public class BindOptionsAttributeTests
{
    [Theory]
    [AutoData]
    public void Ctor_PassedInConfigurationSection_ConfigurationSectionIsExpected
    (
        string configurationSection
    )
    {
        var result = new BindOptionsAttribute(configurationSection);

        result.ConfigurationSection.Should().Be(configurationSection);
    }
}