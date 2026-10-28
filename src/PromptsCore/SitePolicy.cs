namespace PromptsCore;

public static class SitePolicy
{
    public static readonly Uri Production = new("https://prompts.harify.com");
    public const string ProductionHost = "prompts.harify.com";

    /// <summary>
    /// Production site, unless PROMPTS_SITE_URL points at https or local http.
    /// </summary>
    public static Uri ResolvedSiteUrl(IDictionary<string, string>? environment = null)
    {
        environment ??= EnvironmentVariables();
        if (!environment.TryGetValue("PROMPTS_SITE_URL", out var raw) || string.IsNullOrWhiteSpace(raw))
        {
            return Production;
        }

        raw = raw.Trim();
        if (!Uri.TryCreate(raw, UriKind.Absolute, out var url) || string.IsNullOrEmpty(url.Scheme) || string.IsNullOrEmpty(url.Host))
        {
            return Production;
        }

        var scheme = url.Scheme.ToLowerInvariant();
        var host = url.Host.ToLowerInvariant();
        if (scheme == "https")
        {
            return url;
        }

        if (scheme == "http" && (host == "localhost" || host == "127.0.0.1"))
        {
            return url;
        }

        return Production;
    }

    /// <summary>
    /// Main-frame navigations stay on the configured host. about: covers the empty document.
    /// </summary>
    public static bool AllowsNavigation(Uri url, Uri siteUrl)
    {
        if (string.Equals(url.Scheme, "about", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.IsNullOrEmpty(url.Host))
        {
            return false;
        }

        var host = url.Host.ToLowerInvariant();
        if (host == ProductionHost || host == "www.prompts.harify.com")
        {
            return true;
        }

        var siteHost = siteUrl.Host.ToLowerInvariant();
        return host == siteHost;
    }

    private static Dictionary<string, string> EnvironmentVariables()
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
        {
            if (entry.Key is string key && entry.Value is string value)
            {
                result[key] = value;
            }
        }

        return result;
    }
}
