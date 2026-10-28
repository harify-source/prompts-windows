using System.Drawing;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace PromptsWindows;

public sealed class TrayController : IDisposable
{
    private readonly PanelWindow _panel;
    private readonly SettingsWindow _settings;
    private readonly NotifyIcon _icon;
    private bool _disposed;

    public TrayController(PanelWindow panel, SettingsWindow settings)
    {
        _panel = panel;
        _settings = settings;

        _icon = new NotifyIcon
        {
            Text = "Prompts — click to open, right-click for menu",
            Visible = true,
            Icon = SystemIcons.Application,
        };

        _icon.MouseClick += OnMouseClick;
        RebuildContextMenu();
    }

    public Rectangle GetIconRect()
    {
        // NotifyIcon does not expose a reliable screen rect; fall back to work-area corner.
        var area = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1280, 720);
        return new Rectangle(area.Right - 40, area.Bottom - 40, 24, 24);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _icon.Visible = false;
        _icon.Dispose();
    }

    private void OnMouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            Application.Current.Dispatcher.Invoke(() => _panel.ToggleNearTray(GetIconRect()));
        }
    }

    private void RebuildContextMenu()
    {
        var menu = new ContextMenuStrip();
        var open = new ToolStripMenuItem(_panel.IsPanelVisible ? "Hide Prompts" : "Open Prompts");
        open.Click += (_, _) => Application.Current.Dispatcher.Invoke(() => _panel.ToggleNearTray(GetIconRect()));
        menu.Items.Add(open);

        var reload = new ToolStripMenuItem("Reload");
        reload.Click += (_, _) => Application.Current.Dispatcher.Invoke(() => _panel.ReloadNearTray(GetIconRect()));
        menu.Items.Add(reload);

        var login = new ToolStripMenuItem("Open at Login") { Checked = LoginItem.IsEnabled, CheckOnClick = false };
        login.Click += (_, _) =>
        {
            try
            {
                LoginItem.SetEnabled(!LoginItem.IsEnabled);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Couldn't change Open at Login.\n{ex.Message}",
                    "Prompts",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
            }

            RebuildContextMenu();
            Application.Current.Dispatcher.Invoke(() => _settings.RefreshLoginControls());
        };
        menu.Items.Add(login);

        menu.Items.Add(new ToolStripSeparator());

        var settingsItem = new ToolStripMenuItem("Settings…");
        settingsItem.Click += (_, _) => Application.Current.Dispatcher.Invoke(() => _settings.ShowSettings());
        menu.Items.Add(settingsItem);

        menu.Items.Add(new ToolStripSeparator());

        var quit = new ToolStripMenuItem("Quit Prompts");
        quit.Click += (_, _) => Application.Current.Shutdown();
        menu.Items.Add(quit);

        menu.Opening += (_, _) =>
        {
            open.Text = _panel.IsPanelVisible ? "Hide Prompts" : "Open Prompts";
            login.Checked = LoginItem.IsEnabled;
        };

        _icon.ContextMenuStrip = menu;
    }
}
