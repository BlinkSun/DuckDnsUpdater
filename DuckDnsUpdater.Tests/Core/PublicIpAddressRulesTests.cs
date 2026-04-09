using DuckDnsUpdater.Core;
using Xunit;

namespace DuckDnsUpdater.Tests.Core;

public class PublicIpAddressRulesTests
{
    [Theory]
    [InlineData("8.8.8.8", true)]
    [InlineData("1.1.1.1", true)]
    [InlineData("192.168.1.10", false)]
    [InlineData("10.0.0.1", false)]
    [InlineData("127.0.0.1", false)]
    [InlineData("invalid", false)]
    public void IsRoutablePublicIp_V4_Cases(string input, bool expected)
    {
        bool result = PublicIpAddressRules.IsRoutablePublicIp(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("2606:4700:4700::1111", true)]
    [InlineData("::1", false)]
    [InlineData("fe80::1", false)]
    [InlineData("fc00::1", false)]
    public void IsRoutablePublicIp_V6_Cases(string input, bool expected)
    {
        bool result = PublicIpAddressRules.IsRoutablePublicIp(input);
        Assert.Equal(expected, result);
    }
}
