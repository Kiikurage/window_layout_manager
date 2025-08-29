using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace window_layout_manager
{
    public class HotKeyPattern
    {
        public uint Modifiers { get; set; }
        public uint Key { get; set; }

        public override string ToString()
        {
            List<string> parts = new();
            if ((Modifiers & WinAPI.ModControl) != 0) parts.Add("Ctrl");
            if ((Modifiers & WinAPI.ModShift) != 0) parts.Add("Shift");
            if ((Modifiers & WinAPI.ModAlt) != 0) parts.Add("Alt");
            if ((Modifiers & WinAPI.ModWin) != 0) parts.Add("Win");
            parts.Add(((Keys)Key).ToString());
            return string.Join("+", parts);
        }

        static public HotKeyPattern? FromPressedKeys(HashSet<Keys> pressedKeys)
        {
            uint modifiers = 0;
            uint key = 0;

            foreach (var pressedKey in pressedKeys)
            {
                switch (pressedKey)
                {
                    case Keys.Control:
                    case Keys.ControlKey:
                    case Keys.LControlKey:
                    case Keys.RControlKey:
                        modifiers |= WinAPI.ModControl;
                        continue;
                    case Keys.Shift:
                    case Keys.ShiftKey:
                    case Keys.LShiftKey:
                    case Keys.RShiftKey:
                        modifiers |= WinAPI.ModShift;
                        continue;
                    case Keys.Alt:
                        modifiers |= WinAPI.ModAlt;
                        continue;
                    case Keys.LWin:
                    case Keys.RWin:
                        modifiers |= WinAPI.ModWin;
                        continue;
                    default:
                        key = (uint)pressedKey;
                        continue;
                }
            }
            if (key == 0)
            {
                return null;
            }

            return new HotKeyPattern
            {
                Modifiers = modifiers,
                Key = key,
            };
        }
    }
}
