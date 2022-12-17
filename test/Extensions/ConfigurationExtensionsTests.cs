using AutoFixture.Xunit2;
using AutoIoc.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Xunit;

namespace AutoIoc.UnitTests.Extensions;

public class ConfigurationExtensionsTests
{
    [Theory]
    [AutoData]
    public void GetRequiredConfiguration_SectionMissing_Throws(
        string appSettingsKey
    )
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()!)
            .Build();

        var act = () => configuration.GetRequiredConfiguration<TestConfiguration>(appSettingsKey);

        act.Should().Throw<AutoIocException>()
            .WithMessage($"Missing or invalid configuration section: '{appSettingsKey}'");
    }

    [Theory]
    [AutoData]
    public void GetRequiredConfiguration_WrongType_Throws(
        string appSettingsKey,
        string randomValue
    )
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {appSettingsKey, randomValue}
            }!)
            .Build();

        var act = () => configuration.GetRequiredConfiguration<TestConfiguration>(appSettingsKey);

        act.Should().Throw<AutoIocException>()
            .WithMessage($"Missing or invalid configuration section: '{appSettingsKey}'");
    }

    [Theory]
    [AutoData]
    public void GetRequiredConfiguration_SectionExistsAndValidValue_ReturnsExpected(
        string appSettingsKey,
        TestConfiguration expected
    )
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {$"{appSettingsKey}:{nameof(TestConfiguration.TestString)}", $"{expected.TestString}"},
                {$"{appSettingsKey}:{nameof(TestConfiguration.TestInt)}", $"{expected.TestInt}"}
            }!)
            .Build();

        var result = configuration.GetRequiredConfiguration<TestConfiguration>(appSettingsKey);

        result.Should().BeEquivalentTo(expected);
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class TestConfiguration
    {
        public string? TestString { get; set; }
        public int TestInt { get; set; }
    }
}