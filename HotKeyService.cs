using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace window_layout_manager
{
    /// <summary>
    /// キーイベントを受け取るサービス
    /// </summary>
    public partial class HotKeyService : Form, IDisposable
    {
        private readonly PreferenceRepository preferenceRepository;
        private readonly Dictionary<int, Command> registeredHotKeys = [];

        public HotKeyService(PreferenceRepository preferenceRepository)
        {
            this.preferenceRepository = preferenceRepository;

            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
            Visible = false;

            LoadHotKeyPreference(Command.TOP_LEFT);
            LoadHotKeyPreference(Command.TOP_RIGHT);
            LoadHotKeyPreference(Command.BOTTOM_LEFT);
            LoadHotKeyPreference(Command.BOTTOM_RIGHT);
            LoadHotKeyPreference(Command.MAXIMIZE);
            LoadHotKeyPreference(Command.LEFT);
            LoadHotKeyPreference(Command.TOP);
            LoadHotKeyPreference(Command.RIGHT);
            LoadHotKeyPreference(Command.BOTTOM);

            preferenceRepository.HotKeyChanged += (s, command, oldPattern, newPattern) =>
            {
                UnregisterHotKey(command);
                RegisterHotKey(command, newPattern);
            };
        }

        private void LoadHotKeyPreference(Command command)
        {
            var pattern = preferenceRepository.GetHotKeyPattern(command);
            if (pattern != null)
            {
                RegisterHotKey(command, pattern);
            }
        }

        private void RegisterHotKey(Command command, HotKeyPattern pattern)
        {
            WinAPI.RegisterHotKey(Handle, command.HotKeyId, pattern.Modifiers, pattern.Key);
            registeredHotKeys.Add(command.HotKeyId, command);
        }

        private void UnregisterHotKey(Command command)
        {
            WinAPI.UnregisterHotKey(Handle, command.HotKeyId);
            registeredHotKeys.Remove(command.HotKeyId);
        }

        private void UnregisterAllHotKeys()
        {
            foreach (var command in registeredHotKeys.Values)
            {
                WinAPI.UnregisterHotKey(Handle, command.HotKeyId);
            }
            registeredHotKeys.Clear();
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY)
            {
                int hotKeyId = m.WParam.ToInt32();

                if (registeredHotKeys.TryGetValue(hotKeyId, out Command? command))
                {
                    command.Execute();
                }
            }

            base.WndProc(ref m);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnregisterAllHotKeys();
            }

            base.Dispose(disposing);
        }
    }
}
