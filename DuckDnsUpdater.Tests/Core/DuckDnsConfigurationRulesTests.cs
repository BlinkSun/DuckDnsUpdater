using DuckDnsUpdater.Core;
using Xunit;

namespace DuckDnsUpdater.Tests.Core;

public class DuckDnsConfigurationRulesTests
{
    [Theory]
    [InlineData("mydomain", true)]
    [InlineData("my-domain", true)]
    [InlineData("mydomain,otherone", true)]
    [InlineData("-invalid", false)]
    [InlineData("invalid-", false)]
    [InlineData("invalid.domain", false)]
    [InlineData("", false)]
    public void IsValidDuckDnsDomain_ReturnsExpected(string input, bool expected)
    {
        bool result = DuckDnsConfigurationRules.IsValidDuckDnsDomain(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(5, 5)]
    [InlineData(30, 30)]
    [InlineData(120, 60)]
    public void NormalizeRefreshInterval_ClampsCorrectly(int input, int expected)
    {
        int result = DuckDnsConfigurationRules.NormalizeRefreshInterval(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TryValidateConfiguration_ReturnsFalse_WhenTokenInvalid()
    {
        bool result = DuckDnsConfigurationRules.TryValidateConfiguration("mydomain", "abc", out string error);

        Assert.False(result);
        Assert.Equal("Invalid token format.", error);
    }

    [Fact]
    public void TryValidateConfiguration_ReturnsTrue_WhenInputValid()
    {
        bool result = DuckDnsConfigurationRules.TryValidateConfiguration("mydomain", "12345678-1234-1234-1234-1234567890ab", out string error);

        Assert.True(result);
        Assert.Equal(string.Empty, error);
    }
}
