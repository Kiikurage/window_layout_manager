using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace window_layout_manager
{
    /// <summary>
    /// キーイベントを受け取るサービス
    /// </summary>
    public partial class HotKeyService
    {
        struct GridRect
        {
            public double GridW;
            public double GridH;
            public double X;
            public double Y;
            public double W;
            public double H;
        }

        private static GridRect TOP_LEFT = new() { GridW = 2, GridH = 2, X = 0, Y = 0, W = 1, H = 1 };
        private static GridRect TOP_RIGHT = new() { GridW = 2, GridH = 2, X = 1, Y = 0, W = 1, H = 1 };
        private static GridRect BOTTOM_LEFT = new() { GridW = 2, GridH = 2, X = 0, Y = 1, W = 1, H = 1 };
        private static GridRect BOTTOM_RIGHT = new() { GridW = 2, GridH = 2, X = 1, Y = 1, W = 1, H = 1 };
        private static GridRect MAXIMIZE = new() { GridW = 1, GridH = 1, X = 0, Y = 0, W = 1, H = 1 };
        private static GridRect LEFT = new() { GridW = 2, GridH = 1, X = 0, Y = 0, W = 1, H = 1 };
        private static GridRect RIGHT = new() { GridW = 2, GridH = 1, X = 1, Y = 0, W = 1, H = 1 };
        private static GridRect TOP = new() { GridW = 1, GridH = 2, X = 0, Y = 0, W = 1, H = 1 };
        private static GridRect BOTTOM = new() { GridW = 1, GridH = 2, X = 0, Y = 1, W = 1, H = 1 };

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AdjustWindowRectEx(ref RECT lpRect, uint dwStyle, bool bMenu, uint dwExStyle);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        const int GWL_STYLE = -16;
        const int GWL_EXSTYLE = -20;

        [StructLayout(LayoutKind.Sequential)]
        struct MONITORINFO
        {
            public uint cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public readonly int Width => Right - Left;
            public readonly int Height => Bottom - Top;
        }

        /// <summary>
        /// 最も近いモニタを取得
        /// </summary>
        private const uint MONITOR_DEFAULTTONEAREST = 0x0002;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_HOTKEY = 0x0312;

        private const int WS_POPUP = 0x000000;

        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;
        private const uint MOD_NOREPEAT = 0x4000;
        private const uint MOD_KEY_A = 0x41;

        private const int HOTKEY_TOP_LEFT = 1;
        private const int HOTKEY_TOP_RIGHT = 2;
        private const int HOTKEY_BOTTOM_LEFT = 3;
        private const int HOTKEY_BOTTOM_RIGHT = 4;
        private const int HOTKEY_MAXIMIZE = 5;
        private const int HOTKEY_LEFT = 6;
        private const int HOTKEY_TOP = 7;
        private const int HOTKEY_RIGHT = 8;
        private const int HOTKEY_BOTTOM = 9;

        private HwndSource? _source;

        public void Start()
        {
            // サイズ0のウィンドウを用意し、キーイベントを受け取れるようにする
            _source = new HwndSource(new HwndSourceParameters("HotKeyService")
            {
                Width = 0,
                Height = 0,
                PositionX = 0,
                PositionY = 0,
                WindowStyle = WS_POPUP,
            });
            _source.AddHook(HwndHook);

            RegisterHotKey(_source.Handle, HOTKEY_TOP_LEFT, MOD_CONTROL | MOD_SHIFT | MOD_WIN | MOD_NOREPEAT, (uint)Keys.A);
            RegisterHotKey(_source.Handle, HOTKEY_TOP_RIGHT, MOD_CONTROL | MOD_SHIFT | MOD_WIN | MOD_NOREPEAT, (uint)Keys.S);
            RegisterHotKey(_source.Handle, HOTKEY_BOTTOM_LEFT, MOD_CONTROL | MOD_SHIFT | MOD_WIN | MOD_NOREPEAT, (uint)Keys.Z);
            RegisterHotKey(_source.Handle, HOTKEY_BOTTOM_RIGHT, MOD_CONTROL | MOD_SHIFT | MOD_WIN | MOD_NOREPEAT, (uint)Keys.X);
            RegisterHotKey(_source.Handle, HOTKEY_MAXIMIZE, MOD_CONTROL | MOD_SHIFT | MOD_WIN | MOD_NOREPEAT, (uint)Keys.D);
            RegisterHotKey(_source.Handle, HOTKEY_LEFT, MOD_CONTROL | MOD_SHIFT | MOD_WIN | MOD_NOREPEAT, (uint) Keys.Left);
            RegisterHotKey(_source.Handle, HOTKEY_TOP, MOD_CONTROL | MOD_SHIFT | MOD_WIN | MOD_NOREPEAT, (uint)Keys.Up);
            RegisterHotKey(_source.Handle, HOTKEY_RIGHT, MOD_CONTROL | MOD_SHIFT | MOD_WIN | MOD_NOREPEAT, (uint)Keys.Right);
            RegisterHotKey(_source.Handle, HOTKEY_BOTTOM, MOD_CONTROL | MOD_SHIFT | MOD_WIN | MOD_NOREPEAT, (uint)Keys.Down);
        }

        public void Dispose()
        {
            if (_source != null)
            {
                UnregisterHotKey(_source.Handle, HOTKEY_TOP_LEFT);
                UnregisterHotKey(_source.Handle, HOTKEY_TOP_RIGHT);
                UnregisterHotKey(_source.Handle, HOTKEY_BOTTOM_LEFT);
                UnregisterHotKey(_source.Handle, HOTKEY_BOTTOM_RIGHT);
                UnregisterHotKey(_source.Handle, HOTKEY_MAXIMIZE);
                UnregisterHotKey(_source.Handle, HOTKEY_LEFT);
                UnregisterHotKey(_source.Handle, HOTKEY_TOP);
                UnregisterHotKey(_source.Handle, HOTKEY_RIGHT);
                UnregisterHotKey(_source.Handle, HOTKEY_BOTTOM);
                _source = null;
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (id == HOTKEY_TOP_LEFT)
                {
                    OnHotKeyPressed(TOP_LEFT);
                    handled = true;
                }
                else if (id == HOTKEY_TOP_RIGHT)
                {
                    OnHotKeyPressed(TOP_RIGHT);
                    handled = true;
                }
                else if (id == HOTKEY_BOTTOM_LEFT)
                {
                    OnHotKeyPressed(BOTTOM_LEFT);
                    handled = true;
                }
                else if (id == HOTKEY_BOTTOM_RIGHT)
                {
                    OnHotKeyPressed(BOTTOM_RIGHT);
                    handled = true;
                }
                else if (id == HOTKEY_MAXIMIZE)
                {
                    OnHotKeyPressed(MAXIMIZE);
                    handled = true;
                }
                else if (id == HOTKEY_LEFT)
                {
                    OnHotKeyPressed(LEFT);
                    handled = true;
                }
                else if (id == HOTKEY_TOP)
                {
                    OnHotKeyPressed(TOP);
                    handled = true;
                }
                else if (id == HOTKEY_RIGHT)
                {
                    OnHotKeyPressed(RIGHT);
                    handled = true;
                }
                else if (id == HOTKEY_BOTTOM)
                {
                    OnHotKeyPressed(BOTTOM);
                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        private void OnHotKeyPressed(GridRect rect)
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero)
            {
                return;
            }
            Resize(hWnd, rect);
        }

        private void Resize(IntPtr hWnd, GridRect rect)
        {
            IntPtr hMonitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);
            MONITORINFO mi = new MONITORINFO();
            mi.cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFO));
            GetMonitorInfo(hMonitor, ref mi);

            double left = mi.rcWork.Left + mi.rcWork.Width * rect.X / rect.GridW;
            double top = mi.rcWork.Top + mi.rcWork.Height * rect.Y / rect.GridH;
            double width = mi.rcWork.Width * rect.W / rect.GridW;
            double height = mi.rcWork.Height * rect.H / rect.GridH;

            RECT clientRect = new() {
                Left = (int)left,
                Top = (int)top,
                Right = (int)(left + width),
                Bottom =(int)(top + height)
            };

            uint style = (uint)GetWindowLong(hWnd, GWL_STYLE);
            uint exStyle = (uint)GetWindowLong(hWnd, GWL_EXSTYLE);

            RECT windowRect = clientRect;
            AdjustWindowRectEx(ref windowRect, style, false, exStyle);

            int topMargin = Math.Abs(clientRect.Top - windowRect.Top);

            MoveWindow(
                hWnd,
                windowRect.Left,
                clientRect.Top,
                windowRect.Width,
                windowRect.Height - topMargin,
                true
            );
        }
    }
}
