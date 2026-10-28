using System.Text.Json;
using System.Text.Json.Serialization;

namespace PromptsCore;

public sealed class GlobalShortcut : IEquatable<GlobalShortcut>
{
    /// <summary>Ctrl+Alt+P (VK_P). Present until the user records or clears a shortcut.</summary>
    public static GlobalShortcut DefaultToggle { get; } = new()
    {
        VirtualKey = 0x50,
        Control = true,
        Option = true,
        Shift = false,
        Command = false,
    };

    public int VirtualKey { get; set; }
    public bool Control { get; set; }
    public bool Option { get; set; }
    public bool Shift { get; set; }
    public bool Command { get; set; }

    /// <summary>A global shortcut needs Control, Option (Alt), or Command (Win) so it does not fire on ordinary typing.</summary>
    [JsonIgnore]
    public bool IsValid => Control || Option || Command;

    /// <summary>Win32 RegisterHotKey modifier flags.</summary>
    [JsonIgnore]
    public uint WinModifiers
    {
        get
        {
            uint flags = 0;
            if (Control) flags |= 0x0002; // MOD_CONTROL
            if (Shift) flags |= 0x0004; // MOD_SHIFT
            if (Option) flags |= 0x0001; // MOD_ALT
            if (Command) flags |= 0x0008; // MOD_WIN
            return flags;
        }
    }

    [JsonIgnore]
    public string DisplayString
    {
        get
        {
            var parts = new List<string>();
            if (Control) parts.Add("Ctrl");
            if (Option) parts.Add("Alt");
            if (Shift) parts.Add("Shift");
            if (Command) parts.Add("Win");
            parts.Add(KeyLabel(VirtualKey));
            return string.Join("+", parts);
        }
    }

    public static string KeyLabel(int virtualKey) =>
        Labels.TryGetValue(virtualKey, out var label) ? label : $"Key {virtualKey}";

    private static readonly Dictionary<int, string> Labels = new()
    {
        [0x41] = "A", [0x42] = "B", [0x43] = "C", [0x44] = "D", [0x45] = "E", [0x46] = "F",
        [0x47] = "G", [0x48] = "H", [0x49] = "I", [0x4A] = "J", [0x4B] = "K", [0x4C] = "L",
        [0x4D] = "M", [0x4E] = "N", [0x4F] = "O", [0x50] = "P", [0x51] = "Q", [0x52] = "R",
        [0x53] = "S", [0x54] = "T", [0x55] = "U", [0x56] = "V", [0x57] = "W", [0x58] = "X",
        [0x59] = "Y", [0x5A] = "Z",
        [0x30] = "0", [0x31] = "1", [0x32] = "2", [0x33] = "3", [0x34] = "4",
        [0x35] = "5", [0x36] = "6", [0x37] = "7", [0x38] = "8", [0x39] = "9",
        [0x20] = "Space", [0x0D] = "Enter", [0x09] = "Tab", [0x08] = "Backspace", [0x1B] = "Escape",
        [0x25] = "←", [0x27] = "→", [0x28] = "↓", [0x26] = "↑",
        [0x70] = "F1", [0x71] = "F2", [0x72] = "F3", [0x73] = "F4", [0x74] = "F5", [0x75] = "F6",
        [0x76] = "F7", [0x77] = "F8", [0x78] = "F9", [0x79] = "F10", [0x7A] = "F11", [0x7B] = "F12",
    };

    public bool Equals(GlobalShortcut? other)
    {
        if (other is null) return false;
        return VirtualKey == other.VirtualKey
            && Control == other.Control
            && Option == other.Option
            && Shift == other.Shift
            && Command == other.Command;
    }

    public override bool Equals(object? obj) => obj is GlobalShortcut other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(VirtualKey, Control, Option, Shift, Command);

    public static bool operator ==(GlobalShortcut? left, GlobalShortcut? right) => Equals(left, right);

    public static bool operator !=(GlobalShortcut? left, GlobalShortcut? right) => !Equals(left, right);
}

public static class ShortcutStore
{
    public const string FileName = "toggle-shortcut.json";

    public static GlobalShortcut? Load(string? storeDirectory = null)
    {
        var path = StorePath(storeDirectory);
        if (!File.Exists(path))
        {
            return GlobalShortcut.DefaultToggle;
        }

        var json = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(json) || json.Trim() == "null")
        {
            return null;
        }

        try
        {
            var shortcut = JsonSerializer.Deserialize<GlobalShortcut>(json);
            if (shortcut is null || !shortcut.IsValid)
            {
                return GlobalShortcut.DefaultToggle;
            }

            return shortcut;
        }
        catch (JsonException)
        {
            return GlobalShortcut.DefaultToggle;
        }
    }

    public static void Save(GlobalShortcut? shortcut, string? storeDirectory = null)
    {
        var dir = storeDirectory ?? DefaultStoreDirectory();
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, FileName);
        if (shortcut is null)
        {
            File.WriteAllText(path, "null");
            return;
        }

        File.WriteAllText(path, JsonSerializer.Serialize(shortcut));
    }

    public static string DefaultStoreDirectory()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(root, "Prompts");
    }

    private static string StorePath(string? storeDirectory) =>
        Path.Combine(storeDirectory ?? DefaultStoreDirectory(), FileName);
}
