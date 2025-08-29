using Microsoft.Win32;
using System.Windows.Forms;

namespace window_layout_manager
{
    public class HotKeyPattern
    {
        public uint Modifiers;
        public uint Key;
    }

    public delegate void HotKeyChangedEventHandler(
        object? sender,
        Command command,
        HotKeyPattern? oldPattern,
        HotKeyPattern newPattern
    );

    public class PreferenceRepository
    {
        public event HotKeyChangedEventHandler? HotKeyChanged;

        private const string RunKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string appName = "window_layout_manager";
        private readonly string exePath = Environment.ProcessPath!;
        private Dictionary<Command, HotKeyPattern> defaultHotKeyPatterns;

        public PreferenceRepository()
        {
            defaultHotKeyPatterns = new()
            {
                { Command.TOP_LEFT, new HotKeyPattern { Modifiers = WinAPI.ModControl | WinAPI.ModShift | WinAPI.ModWin | WinAPI.ModNoRepeat, Key = (uint)Keys.A } },
                { Command.TOP_RIGHT, new HotKeyPattern { Modifiers = WinAPI.ModControl | WinAPI.ModShift | WinAPI.ModWin | WinAPI.ModNoRepeat, Key = (uint)Keys.S } },
                { Command.BOTTOM_LEFT, new HotKeyPattern { Modifiers = WinAPI.ModControl | WinAPI.ModShift | WinAPI.ModWin | WinAPI.ModNoRepeat, Key = (uint)Keys.Z } },
                { Command.BOTTOM_RIGHT, new HotKeyPattern { Modifiers = WinAPI.ModControl | WinAPI.ModShift | WinAPI.ModWin | WinAPI.ModNoRepeat, Key = (uint)Keys.X } },
                { Command.MAXIMIZE, new HotKeyPattern { Modifiers = WinAPI.ModControl | WinAPI.ModShift | WinAPI.ModWin | WinAPI.ModNoRepeat, Key = (uint)Keys.D } },
                { Command.LEFT, new HotKeyPattern { Modifiers = WinAPI.ModControl | WinAPI.ModShift | WinAPI.ModWin | WinAPI.ModNoRepeat, Key = (uint)Keys.Left } },
                { Command.TOP, new HotKeyPattern { Modifiers = WinAPI.ModControl | WinAPI.ModShift | WinAPI.ModWin | WinAPI.ModNoRepeat, Key = (uint)Keys.Up } },
                { Command.RIGHT, new HotKeyPattern { Modifiers = WinAPI.ModControl | WinAPI.ModShift | WinAPI.ModWin | WinAPI.ModNoRepeat, Key = (uint)Keys.Right } },
                { Command.BOTTOM, new HotKeyPattern { Modifiers = WinAPI.ModControl | WinAPI.ModShift | WinAPI.ModWin | WinAPI.ModNoRepeat, Key = (uint)Keys.Down } },
            };

        }
        public void EnableAutoStart()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
            if (key == null)
                throw new InvalidOperationException("レジストリの設定に失敗しました");

            key.SetValue(appName, exePath);
        }

        public void DisableAutoStart()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
            if (key == null)
                throw new InvalidOperationException("レジストリの設定に失敗しました");

            key.DeleteValue(appName, false);
        }

        public bool IsAutoStartEnabled()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
            if (key == null) return false;

            var value = key.GetValue(appName) as string;
            return value == exePath;
        }

        public void SetHotKeyPattern(Command command, HotKeyPattern newPattern)
        {
            // 今のところレジストリに保存しない
            // 将来的にUIで変更できるようにする場合は保存する

            var oldPattern = GetHotKeyPattern(command);
            HotKeyChanged?.Invoke(this, command, oldPattern, newPattern);
        }

        public HotKeyPattern? GetHotKeyPattern(Command command)
        {
            defaultHotKeyPatterns.TryGetValue(command, out var pattern);
            return pattern;
        }
    }
}
