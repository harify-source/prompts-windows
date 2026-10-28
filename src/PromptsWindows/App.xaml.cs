using System.Windows;
using PromptsCore;

namespace PromptsWindows;

public partial class App : System.Windows.Application
{
    private TrayController? _tray;
    private PanelWindow? _panel;
    private SettingsWindow? _settings;
    private HotKeyCenter? _hotKeys;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var siteUrl = SitePolicy.ResolvedSiteUrl();
        _panel = new PanelWindow(siteUrl);
        _settings = new SettingsWindow();
        _tray = new TrayController(_panel, _settings);

        _hotKeys = HotKeyCenter.Instance;
        _hotKeys.Pressed += (_, _) => Dispatcher.Invoke(() => _panel.ToggleNearTray(_tray.GetIconRect()));
        _hotKeys.Start();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotKeys?.Dispose();
        _tray?.Dispose();
        base.OnExit(e);
    }
}
