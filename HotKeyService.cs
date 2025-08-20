using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace window_layout_manager
{
    class HotKey
    {
        public const int TOP_LEFT = 1;
        public const int TOP_RIGHT = 2;
        public const int BOTTOM_LEFT = 3;
        public const int BOTTOM_RIGHT = 4;
        public const int MAXIMIZE = 5;
        public const int LEFT = 6;
        public const int TOP = 7;
        public const int RIGHT = 8;
        public const int BOTTOM = 9;
    }

    /// <summary>
    /// キーイベントを受け取るサービス
    /// </summary>
    public partial class HotKeyService : Form, IDisposable
    {
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

        [StructLayout(LayoutKind.Sequential)]
        struct MONITORINFO
        {
            public uint cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        /// <summary>
        /// 最も近いモニタを取得
        /// </summary>
        private const uint MONITOR_DEFAULTTONEAREST = 0x0002;
        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;
        private const int SW_RESTORE = 9;

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

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsZoomed(IntPtr hWnd);

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

        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;
        private const uint MOD_NOREPEAT = 0x4000;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private static NotifyIcon? _notifyIcon;

        public HotKeyService()
        {
            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
            Visible = false;

            RegisterHotKey(Handle, HotKey.TOP_LEFT, MOD_CONTROL | MOD_SHIFT | MOD_WIN | MOD_NOREPEAT, (uint)Keys.A);
            RegisterHotKey(Handle, HotKey.TOP_RIGHT, MOD_CONTROL | MOD_SHIFT | MOD_WIN | MOD_NOREPEAT, (uint)Keys.S);
            RegisterHotKey(Handle, HotKey.BOTTOM_LEFT, MOD_CONTROL | MOD_SHIFT | MOD_WIN | MOD_NOREPEAT, (uint)Keys.Z);
            RegisterHotKey(Handle, HotKey.BOTTOM_RIGHT, MOD_CONTROL | MOD_SHIFT | MOD_WIN | MOD_NOREPEAT, (uint)Keys.X);
            RegisterHotKey(Handle, HotKey.MAXIMIZE, MOD_CONTROL | MOD_SHIFT | MOD_WIN | MOD_NOREPEAT, (uint)Keys.D);
            RegisterHotKey(Handle, HotKey.LEFT, MOD_CONTROL | MOD_SHIFT | MOD_WIN | MOD_NOREPEAT, (uint)Keys.Left);
            RegisterHotKey(Handle, HotKey.TOP, MOD_CONTROL | MOD_SHIFT | MOD_WIN | MOD_NOREPEAT, (uint)Keys.Up);
            RegisterHotKey(Handle, HotKey.RIGHT, MOD_CONTROL | MOD_SHIFT | MOD_WIN | MOD_NOREPEAT, (uint)Keys.Right);
            RegisterHotKey(Handle, HotKey.BOTTOM, MOD_CONTROL | MOD_SHIFT | MOD_WIN | MOD_NOREPEAT, (uint)Keys.Down);

            _notifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application,
                Visible = true,
                Text = "Window Layout Manager"
            };

            var menu = new ContextMenuStrip();
            menu.Items.Add("Exit", null, (s, ev) =>
            {
                _notifyIcon.Visible = false;
                Dispose();
                Application.Exit();
            });
            _notifyIcon.ContextMenuStrip = menu;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();

                if (id == HotKey.TOP_LEFT)
                {
                    OnHotKeyPressed(TOP_LEFT);
                }
                else if (id == HotKey.TOP_RIGHT)
                {
                    OnHotKeyPressed(TOP_RIGHT);
                }
                else if (id == HotKey.BOTTOM_LEFT)
                {
                    OnHotKeyPressed(BOTTOM_LEFT);
                }
                else if (id == HotKey.BOTTOM_RIGHT)
                {
                    OnHotKeyPressed(BOTTOM_RIGHT);
                }
                else if (id == HotKey.MAXIMIZE)
                {
                    OnHotKeyPressed(MAXIMIZE);
                }
                else if (id == HotKey.LEFT)
                {
                    OnHotKeyPressed(LEFT);
                }
                else if (id == HotKey.TOP)
                {
                    OnHotKeyPressed(TOP);
                }
                else if (id == HotKey.RIGHT)
                {
                    OnHotKeyPressed(RIGHT);
                }
                else if (id == HotKey.BOTTOM)
                {
                    OnHotKeyPressed(BOTTOM);
                }
            }

            base.WndProc(ref m);
        }

        private void OnHotKeyPressed(GridRect rect)
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero)
            {
                return;
            }
            ResizeWindow(hWnd, rect);
        }

        private void ResizeWindow(IntPtr hWnd, GridRect rect)
        {
            if (IsZoomed(hWnd))
            {
                ShowWindow(hWnd, SW_RESTORE);
            }

            IntPtr hMonitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);
            MONITORINFO mi = new MONITORINFO();
            mi.cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFO));
            GetMonitorInfo(hMonitor, ref mi);

            double left = mi.rcWork.Left + mi.rcWork.Width * rect.X / rect.GridW;
            double top = mi.rcWork.Top + mi.rcWork.Height * rect.Y / rect.GridH;
            double width = mi.rcWork.Width * rect.W / rect.GridW;
            double height = mi.rcWork.Height * rect.H / rect.GridH;

            RECT clientRect = new()
            {
                Left = (int)left,
                Top = (int)top,
                Right = (int)(left + width),
                Bottom = (int)(top + height)
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.Dispose();
                    _notifyIcon = null;
                }

                UnregisterHotKey(Handle, HotKey.TOP_LEFT);
                UnregisterHotKey(Handle, HotKey.TOP_RIGHT);
                UnregisterHotKey(Handle, HotKey.BOTTOM_LEFT);
                UnregisterHotKey(Handle, HotKey.BOTTOM_RIGHT);
                UnregisterHotKey(Handle, HotKey.MAXIMIZE);
                UnregisterHotKey(Handle, HotKey.LEFT);
                UnregisterHotKey(Handle, HotKey.TOP);
                UnregisterHotKey(Handle, HotKey.RIGHT);
                UnregisterHotKey(Handle, HotKey.BOTTOM);
            }

            base.Dispose(disposing);
        }
    }
}
