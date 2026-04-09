using System.Net;
using System.Net.Sockets;

namespace DuckDnsUpdater.Core;

internal static class PublicIpAddressRules
{
    public static bool IsRoutablePublicIp(string ipText)
    {
        if (!IPAddress.TryParse(ipText, out IPAddress? ipAddress))
        {
            return false;
        }

        if (IPAddress.IsLoopback(ipAddress) || ipAddress.Equals(IPAddress.Any) || ipAddress.Equals(IPAddress.IPv6Any))
        {
            return false;
        }

        byte[] bytes = ipAddress.GetAddressBytes();

        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            byte b0 = bytes[0];
            byte b1 = bytes[1];

            if (b0 == 10) return false;
            if (b0 == 127) return false;
            if (b0 == 169 && b1 == 254) return false;
            if (b0 == 172 && b1 >= 16 && b1 <= 31) return false;
            if (b0 == 192 && b1 == 168) return false;
            if (b0 == 100 && b1 >= 64 && b1 <= 127) return false;
            if (b0 == 198 && (b1 == 18 || b1 == 19)) return false;
            if (b0 == 0 || b0 >= 224) return false;

            return true;
        }

        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (ipAddress.IsIPv6LinkLocal || ipAddress.IsIPv6Multicast || ipAddress.IsIPv6SiteLocal || ipAddress.Equals(IPAddress.IPv6Loopback))
            {
                return false;
            }

            if ((bytes[0] & 0xFE) == 0xFC)
            {
                return false;
            }

            return true;
        }

        return false;
    }
}
