using Microsoft.Win32;
using System.Windows.Forms;

namespace window_layout_manager
{
    public class CommandEntry
    {
        public int CommandId { get; set; }
        public required HotKeyPattern Pattern { get; set; }
    }

    public class Configuration
    {
        public List<CommandEntry> Entries { get; set; } = [];

        public Configuration Add(int commandId, HotKeyPattern pattern)
        {
            Entries.Add(new()
            {
                CommandId = commandId,
                Pattern = pattern
            });
            return this;
        }
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
        private const string AppName = "window_layout_manager";
        private readonly string exePath = Environment.ProcessPath!;

        private readonly Configuration configuration = GetDefaultConfiguration();

        private string ConfigFilePath
        {
            get {
                return Path.Combine(
                    Path.GetDirectoryName(exePath)!,
                    AppName + ".config.json"
                );
            }
        }

        public PreferenceRepository()
        {
            //var config = new Configuration();
            //config.Entries.Add(new CommandEntry {
            //    CommandId = Command.TOP_LEFT.HotKeyId,
            //    Pattern = new HotKeyPattern
            //    {
            //        Modifiers = WinAPI.ModControl | WinAPI.ModShift | WinAPI.ModWin | WinAPI.ModNoRepeat,
            //        Key = (uint)Keys.A
            //    }
            //});

            //string json = JsonSerializer.Serialize(config);
            //Debug.WriteLine(json);

            //var deserializedConfig = JsonSerializer.Deserialize<Configuration>(json);
        }
        
        public void EnableAutoStart()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
            if (key == null)
                throw new InvalidOperationException("レジストリの設定に失敗しました");

            key.SetValue(AppName, exePath);
        }

        public void DisableAutoStart()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
            if (key == null)
                throw new InvalidOperationException("レジストリの設定に失敗しました");

            key.DeleteValue(AppName, false);
        }

        public bool IsAutoStartEnabled()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
            if (key == null) return false;

            var value = key.GetValue(AppName) as string;
            return value == exePath;
        }

        public void SetHotKeyPattern(Command command, HotKeyPattern newPattern)
        {
            
            var oldPattern = GetHotKeyPattern(command);
            if (oldPattern == null)
            {
                configuration.Entries.Add(new CommandEntry
                {
                    CommandId = command.HotKeyId,
                    Pattern = newPattern
                });
            }
            else
            {
                foreach (var entry in configuration.Entries)
                {
                    if (entry.CommandId == command.HotKeyId)
                    {
                        entry.Pattern = newPattern;
                        break;
                    }
                }
            }

            // TODO 保存

            HotKeyChanged?.Invoke(this, command, oldPattern, newPattern);
        }

        public HotKeyPattern? GetHotKeyPattern(Command command)
        {
            foreach (var entry in configuration.Entries)
            {
                if (entry.CommandId == command.HotKeyId)
                {
                    return entry.Pattern;
                }
            }
            return null;
        }

        private static Configuration GetDefaultConfiguration()
        {
            return new Configuration()
                .Add(Command.TOP_LEFT.HotKeyId, new()
                {
                    Modifiers = WinAPI.ModControl | WinAPI.ModShift | WinAPI.ModWin | WinAPI.ModNoRepeat,
                    Key = (uint)Keys.A
                })
                .Add(Command.TOP_RIGHT.HotKeyId, new()
                {
                    Modifiers = WinAPI.ModControl | WinAPI.ModShift | WinAPI.ModWin | WinAPI.ModNoRepeat,
                    Key = (uint)Keys.S
                })
                .Add(Command.BOTTOM_LEFT.HotKeyId, new()
                {
                    Modifiers = WinAPI.ModControl | WinAPI.ModShift | WinAPI.ModWin | WinAPI.ModNoRepeat,
                    Key = (uint)Keys.Z
                })
                .Add(Command.BOTTOM_RIGHT.HotKeyId, new()
                {
                    Modifiers = WinAPI.ModControl | WinAPI.ModShift | WinAPI.ModWin | WinAPI.ModNoRepeat,
                    Key = (uint)Keys.X
                })
                .Add(Command.MAXIMIZE.HotKeyId, new()
                {
                    Modifiers = WinAPI.ModControl | WinAPI.ModShift | WinAPI.ModWin | WinAPI.ModNoRepeat,
                    Key = (uint)Keys.D
                })
                .Add(Command.LEFT.HotKeyId, new()
                {
                    Modifiers = WinAPI.ModControl | WinAPI.ModShift | WinAPI.ModWin | WinAPI.ModNoRepeat,
                    Key = (uint)Keys.Left
                })
                .Add(Command.TOP.HotKeyId, new()
                {
                    Modifiers = WinAPI.ModControl | WinAPI.ModShift | WinAPI.ModWin | WinAPI.ModNoRepeat,
                    Key = (uint)Keys.Up
                })
                .Add(Command.RIGHT.HotKeyId, new()
                {
                    Modifiers = WinAPI.ModControl | WinAPI.ModShift | WinAPI.ModWin | WinAPI.ModNoRepeat,
                    Key = (uint)Keys.Right
                })
                .Add(Command.BOTTOM.HotKeyId, new()
                {
                    Modifiers = WinAPI.ModControl | WinAPI.ModShift | WinAPI.ModWin | WinAPI.ModNoRepeat,
                    Key = (uint)Keys.Down
                });
        }
    }
}
