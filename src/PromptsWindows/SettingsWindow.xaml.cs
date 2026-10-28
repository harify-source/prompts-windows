using System.Windows;
using System.Windows.Input;
using PromptsCore;

namespace PromptsWindows;

public partial class SettingsWindow : Window
{
    private bool _recording;
    private GlobalShortcut? _shortcut;
    private bool _updatingLoginToggle;

    public SettingsWindow()
    {
        InitializeComponent();
        Closing += (_, e) =>
        {
            e.Cancel = true;
            Hide();
        };
        _shortcut = ShortcutStore.Load();
        UpdateRecorderLabel();
        RefreshLoginControls();
        ShortcutStatus.Text = StatusText(_shortcut, HotKeyCenter.Instance.IsRegistered);
    }

    public void ShowSettings()
    {
        RefreshLoginControls();
        if (!IsVisible)
        {
            Show();
        }

        Activate();
    }

    public void RefreshLoginControls()
    {
        _updatingLoginToggle = true;
        LoginToggle.IsChecked = LoginItem.IsEnabled;
        _updatingLoginToggle = false;
        LoginStatus.Text = string.Empty;
    }

    private void OnRecorderClick(object sender, MouseButtonEventArgs e)
    {
        _recording = true;
        RecorderLabel.Text = "Press shortcut";
        RecorderBorder.BorderBrush = System.Windows.Media.Brushes.DodgerBlue;
        RecorderBorder.Focus();
        Keyboard.Focus(RecorderBorder);
    }

    private void OnRecorderKeyDown(object sender, KeyEventArgs e)
    {
        if (!_recording) return;
        e.Handled = true;

        if (e.Key == Key.Escape)
        {
            EndRecording();
            return;
        }

        if (e.Key is Key.Back or Key.Delete)
        {
            Apply(null);
            EndRecording();
            return;
        }

        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt
            or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin)
        {
            return;
        }

        var modifiers = Keyboard.Modifiers;
        var shortcut = new GlobalShortcut
        {
            VirtualKey = KeyInterop.VirtualKeyFromKey(key),
            Control = modifiers.HasFlag(ModifierKeys.Control),
            Option = modifiers.HasFlag(ModifierKeys.Alt),
            Shift = modifiers.HasFlag(ModifierKeys.Shift),
            Command = modifiers.HasFlag(ModifierKeys.Windows),
        };

        if (!shortcut.IsValid)
        {
            System.Media.SystemSounds.Beep.Play();
            return;
        }

        Apply(shortcut);
        EndRecording();
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        Apply(null);
        EndRecording();
    }

    private void OnLoginToggle(object sender, RoutedEventArgs e)
    {
        if (_updatingLoginToggle) return;
        var enable = LoginToggle.IsChecked == true;
        try
        {
            LoginItem.SetEnabled(enable);
            LoginStatus.Text = string.Empty;
        }
        catch (Exception ex)
        {
            _updatingLoginToggle = true;
            LoginToggle.IsChecked = LoginItem.IsEnabled;
            _updatingLoginToggle = false;
            LoginStatus.Text = $"Couldn't change Open at Login. {ex.Message}";
        }
    }

    private void Apply(GlobalShortcut? shortcut)
    {
        _shortcut = shortcut;
        ShortcutStore.Save(shortcut);
        var registered = HotKeyCenter.Instance.Register(shortcut);
        UpdateRecorderLabel();
        ShortcutStatus.Text = StatusText(shortcut, registered);
    }

    private void EndRecording()
    {
        _recording = false;
        RecorderBorder.BorderBrush = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(0xCC, 0xCC, 0xCC));
        UpdateRecorderLabel();
    }

    private void UpdateRecorderLabel() =>
        RecorderLabel.Text = _shortcut?.DisplayString ?? "None";

    private static string StatusText(GlobalShortcut? shortcut, bool registered)
    {
        if (shortcut is null) return "No shortcut assigned.";
        if (registered) return string.Empty;
        return "Windows is already using that shortcut.";
    }
}
