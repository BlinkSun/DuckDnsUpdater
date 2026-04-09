using System.Security.Cryptography;
using System.Text;

namespace DuckDnsUpdater;

internal static class TokenProtector
{
    private const string Prefix = "dpapi:";

    public static string Protect(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
        {
            return string.Empty;
        }

        if (plainText.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return plainText;
        }

        byte[] bytes = Encoding.UTF8.GetBytes(plainText);
        byte[] protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        return $"{Prefix}{Convert.ToBase64String(protectedBytes)}";
    }

    public static string Unprotect(string? storedValue)
    {
        if (string.IsNullOrWhiteSpace(storedValue))
        {
            return string.Empty;
        }

        if (!storedValue.StartsWith(Prefix, StringComparison.Ordinal))
        {
            // Backward compatibility with legacy plain-text settings.
            return storedValue;
        }

        try
        {
            string payload = storedValue[Prefix.Length..];
            byte[] protectedBytes = Convert.FromBase64String(payload);
            byte[] plainBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch
        {
            return string.Empty;
        }
    }
}
