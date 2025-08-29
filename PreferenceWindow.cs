using System.Diagnostics;
using System.Windows.Forms;

namespace window_layout_manager
{
    public class PreferenceWindow : Form
    {
        readonly HashSet<Keys> pressedKeys = [];

        public PreferenceWindow()
        {
            Text = "Preferences";
            Show();

            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
        }

        private void OnKeyDown(Object? sender, KeyEventArgs ev)
        {
            pressedKeys.Add(ev.KeyCode);

            var pattern = HotKeyPattern.FromPressedKeys(pressedKeys);
            if (pattern == null)
            {
                return;
            }

            Debug.WriteLine($"{pattern}");
        }

        private void OnKeyUp(Object? sender, KeyEventArgs ev)
        {
            pressedKeys.Remove(ev.KeyCode);
        }
    }
}
