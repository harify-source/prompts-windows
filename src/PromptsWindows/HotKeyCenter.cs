using System.Runtime.InteropServices;
using System.Windows.Interop;
using PromptsCore;

namespace PromptsWindows;

/// <summary>Registers a system-wide hotkey via Win32 RegisterHotKey.</summary>
public sealed class HotKeyCenter : IDisposable
{
    public static HotKeyCenter Instance { get; } = new();

    public event EventHandler? Pressed;
    public bool IsRegistered { get; private set; }

    private const int HotKeyId = 1;
    private HwndSource? _source;
    private bool _disposed;

    private HotKeyCenter() { }

    public void Start()
    {
        EnsureWindow();
        Register(ShortcutStore.Load());
    }

    public bool Register(GlobalShortcut? shortcut)
    {
        EnsureWindow();
        if (_source?.Handle is { } hwnd)
        {
            UnregisterHotKey(hwnd, HotKeyId);
        }

        if (shortcut is null || !shortcut.IsValid)
        {
            IsRegistered = false;
            return shortcut is null;
        }

        if (_source is null)
        {
            IsRegistered = false;
            return false;
        }

        var ok = RegisterHotKey(_source.Handle, HotKeyId, shortcut.WinModifiers, (uint)shortcut.VirtualKey);
        IsRegistered = ok;
        return ok;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_source?.Handle is { } hwnd)
        {
            UnregisterHotKey(hwnd, HotKeyId);
            _source.RemoveHook(WndProc);
            _source.Dispose();
            _source = null;
        }
    }

    private void EnsureWindow()
    {
        if (_source is not null) return;
        var parameters = new HwndSourceParameters("PromptsHotKey")
        {
            Width = 0,
            Height = 0,
            PositionX = 0,
            PositionY = 0,
            WindowStyle = unchecked((int)0x80000000), // WS_POPUP
        };
        _source = new HwndSource(parameters);
        _source.AddHook(WndProc);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int wmHotkey = 0x0312;
        if (msg == wmHotkey && wParam.ToInt32() == HotKeyId)
        {
            Pressed?.Invoke(this, EventArgs.Empty);
            handled = true;
        }

        return IntPtr.Zero;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
