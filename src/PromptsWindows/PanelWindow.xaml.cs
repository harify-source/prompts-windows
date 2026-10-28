using System.Drawing;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using PromptsCore;

namespace PromptsWindows;

public partial class PanelWindow : Window
{
    private readonly Uri _siteUrl;
    private readonly TimeSpan _refreshInterval = TimeSpan.FromMinutes(15);
    private bool _hasContent;
    private DateTime? _lastLoadAt;
    private bool _ready;
    private bool _suppressDeactivateClose;

    public PanelWindow(Uri siteUrl)
    {
        InitializeComponent();
        _siteUrl = siteUrl;
        OfflineDetail.Text = $"{siteUrl.Host} didn't load. Check the connection and try again.";
        Loaded += async (_, _) => await EnsureWebViewAsync();
    }

    public bool IsPanelVisible => IsVisible;

    public void ToggleNearTray(Rectangle trayHint)
    {
        if (IsVisible)
        {
            Hide();
            return;
        }

        ShowNearTray(trayHint);
    }

    public void ShowNearTray(Rectangle trayHint)
    {
        PositionNearTray(trayHint);
        RefreshIfStale();
        _suppressDeactivateClose = true;
        Show();
        Activate();
        _ = Dispatcher.BeginInvoke(() => _suppressDeactivateClose = false);
    }

    public void ReloadNearTray(Rectangle trayHint)
    {
        _ = LoadAsync();
        if (!IsVisible)
        {
            ShowNearTray(trayHint);
        }
    }

    private async Task EnsureWebViewAsync()
    {
        if (_ready) return;
        await WebView.EnsureCoreWebView2Async();
        WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
        WebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
        WebView.CoreWebView2.Settings.IsStatusBarEnabled = false;
        WebView.CoreWebView2.NavigationStarting += OnNavigationStarting;
        WebView.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
        WebView.CoreWebView2.NewWindowRequested += OnNewWindowRequested;
        _ready = true;
        await LoadAsync();
    }

    private void OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (!Uri.TryCreate(e.Uri, UriKind.Absolute, out var url))
        {
            e.Cancel = true;
            return;
        }

        if (SitePolicy.AllowsNavigation(url, _siteUrl))
        {
            return;
        }

        e.Cancel = true;
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url.AbsoluteUri)
            {
                UseShellExecute = true,
            });
        }
        catch
        {
            // Ignore failures opening external URLs.
        }
    }

    private void OnNewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        e.Handled = true;
        if (!Uri.TryCreate(e.Uri, UriKind.Absolute, out var url))
        {
            return;
        }

        if (SitePolicy.AllowsNavigation(url, _siteUrl))
        {
            WebView.CoreWebView2?.Navigate(url.AbsoluteUri);
            return;
        }

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url.AbsoluteUri)
            {
                UseShellExecute = true,
            });
        }
        catch
        {
            // Ignore failures opening external URLs.
        }
    }

    private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (e.IsSuccess)
        {
            _hasContent = true;
            OfflinePanel.Visibility = Visibility.Collapsed;
            return;
        }

        if (!_hasContent)
        {
            OfflinePanel.Visibility = Visibility.Visible;
        }
    }

    private void RefreshIfStale()
    {
        if (_lastLoadAt is null || DateTime.UtcNow - _lastLoadAt >= _refreshInterval)
        {
            _ = LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        _lastLoadAt = DateTime.UtcNow;
        OfflinePanel.Visibility = Visibility.Collapsed;
        if (!_ready)
        {
            await EnsureWebViewAsync();
            return;
        }

        WebView.CoreWebView2?.Navigate(_siteUrl.AbsoluteUri);
    }

    private void OnRetryClick(object sender, RoutedEventArgs e) => _ = LoadAsync();

    private void OnDeactivated(object? sender, EventArgs e)
    {
        if (_suppressDeactivateClose) return;
        if (IsVisible)
        {
            Hide();
        }
    }

    private void PositionNearTray(Rectangle trayHint)
    {
        var work = SystemParameters.WorkArea;
        var width = Math.Min(960, Math.Max(320, work.Width - 24));
        var height = Math.Min(680, Math.Max(240, work.Height - 16));
        Width = width;
        Height = height;

        var left = trayHint.Right - width;
        var top = trayHint.Top - height - 8;
        if (top < work.Top)
        {
            top = trayHint.Bottom + 8;
        }

        left = Math.Max(work.Left + 8, Math.Min(left, work.Right - width - 8));
        top = Math.Max(work.Top + 8, Math.Min(top, work.Bottom - height - 8));
        Left = left;
        Top = top;
    }
}
