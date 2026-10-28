using PromptsCore;

var failures = new List<string>();

void Expect(bool condition, string message)
{
    if (!condition)
    {
        failures.Add(message);
    }
}

void CheckSitePolicy()
{
    var production = SitePolicy.ResolvedSiteUrl(new Dictionary<string, string>());
    Expect(
        production.Scheme == "https" && production.Host == "prompts.harify.com",
        "default site");

    var local = SitePolicy.ResolvedSiteUrl(new Dictionary<string, string>
    {
        ["PROMPTS_SITE_URL"] = "http://localhost:4310",
    });
    Expect(local.Scheme == "http" && local.Host == "localhost" && local.Port == 4310, "localhost override");

    var rejected = SitePolicy.ResolvedSiteUrl(new Dictionary<string, string>
    {
        ["PROMPTS_SITE_URL"] = "http://example.com",
    });
    Expect(rejected.Host == "prompts.harify.com" && rejected.Scheme == "https", "reject plain http");

    var site = SitePolicy.Production;
    Expect(SitePolicy.AllowsNavigation(site, site), "allow production");
    Expect(
        SitePolicy.AllowsNavigation(new Uri("https://prompts.harify.com/fill"), site),
        "allow same host path");
    var dev = new Uri("http://127.0.0.1:4310");
    Expect(SitePolicy.AllowsNavigation(dev, dev), "allow dev host");
    Expect(
        !SitePolicy.AllowsNavigation(new Uri("https://example.com"), site),
        "block other sites");
}

void CheckShortcuts()
{
    Expect(GlobalShortcut.DefaultToggle.DisplayString == "Ctrl+Alt+P", "default display");
    Expect(GlobalShortcut.DefaultToggle.WinModifiers == (0x0002u | 0x0001u), "default win modifiers");
    Expect(GlobalShortcut.DefaultToggle.IsValid, "default is valid");

    var shiftOnly = new GlobalShortcut
    {
        VirtualKey = 0x50,
        Control = false,
        Option = false,
        Shift = true,
        Command = false,
    };
    Expect(!shiftOnly.IsValid, "shift alone is not a global shortcut");

    var dir = Path.Combine(Path.GetTempPath(), "prompts-windows-checks-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(dir);
    try
    {
        Expect(ShortcutStore.Load(dir) == GlobalShortcut.DefaultToggle, "missing value uses default");

        var custom = new GlobalShortcut
        {
            VirtualKey = 0x41,
            Control = false,
            Option = false,
            Shift = false,
            Command = true,
        };
        ShortcutStore.Save(custom, dir);
        Expect(ShortcutStore.Load(dir) == custom, "round trip");

        ShortcutStore.Save(null, dir);
        Expect(ShortcutStore.Load(dir) is null, "cleared shortcut");
    }
    finally
    {
        try
        {
            Directory.Delete(dir, recursive: true);
        }
        catch
        {
            // best effort
        }
    }
}

CheckSitePolicy();
CheckShortcuts();

if (failures.Count == 0)
{
    Console.WriteLine("SitePolicyChecks passed");
    return 0;
}

foreach (var failure in failures)
{
    Console.Error.WriteLine($"FAIL {failure}");
}

return 1;
