namespace DuckDnsUpdater.Core;

internal static class DuckDnsConfigurationRules
{
    public const int MinRefreshIntervalMinutes = 5;
    public const int MaxRefreshIntervalMinutes = 60;

    public static string NormalizeDomain(string? domain) => domain?.Trim().ToLowerInvariant() ?? string.Empty;

    public static string NormalizeToken(string? token) => token?.Trim() ?? string.Empty;

    public static int NormalizeRefreshInterval(int refreshInterval)
    {
        if (refreshInterval < MinRefreshIntervalMinutes) return MinRefreshIntervalMinutes;
        if (refreshInterval > MaxRefreshIntervalMinutes) return MaxRefreshIntervalMinutes;
        return refreshInterval;
    }

    public static bool TryValidateConfiguration(string domain, string token, out string validationError)
    {
        string normalizedDomain = NormalizeDomain(domain);
        string normalizedToken = NormalizeToken(token);

        if (string.IsNullOrWhiteSpace(normalizedDomain) || string.IsNullOrWhiteSpace(normalizedToken))
        {
            validationError = "Missing domain or token.";
            return false;
        }

        if (!IsValidDuckDnsDomain(normalizedDomain))
        {
            validationError = "Invalid domain format.";
            return false;
        }

        if (!Guid.TryParse(normalizedToken, out _))
        {
            validationError = "Invalid token format.";
            return false;
        }

        validationError = string.Empty;
        return true;
    }

    public static bool IsValidDuckDnsDomain(string domainList)
    {
        string[] domains = domainList.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (domains.Length == 0)
        {
            return false;
        }

        foreach (string domain in domains)
        {
            if (!IsValidSingleDuckDnsSubdomain(domain))
            {
                return false;
            }
        }

        return true;
    }

    public static bool IsValidSingleDuckDnsSubdomain(string domain)
    {
        if (domain.Length is < 1 or > 63)
        {
            return false;
        }

        if (domain.StartsWith('-') || domain.EndsWith('-'))
        {
            return false;
        }

        foreach (char c in domain)
        {
            if (!(char.IsLetterOrDigit(c) || c == '-'))
            {
                return false;
            }
        }

        return true;
    }
}
