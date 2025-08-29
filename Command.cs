using System.Runtime.InteropServices;
using static window_layout_manager.WinAPI;

namespace window_layout_manager
{
    public abstract class Command
    {
        public int HotKeyId;
        abstract public void Execute();

        public static readonly ResizeCommand TOP_LEFT = new() { HotKeyId = 1, Rect = GridRect.TopLeft };
        public static readonly ResizeCommand TOP_RIGHT = new() { HotKeyId = 2, Rect = GridRect.TopRight };
        public static readonly ResizeCommand BOTTOM_LEFT = new() { HotKeyId = 3, Rect = GridRect.BottomLeft };
        public static readonly ResizeCommand BOTTOM_RIGHT = new() { HotKeyId = 4, Rect = GridRect.BottomRight };
        public static readonly ResizeCommand MAXIMIZE = new() { HotKeyId = 5, Rect = GridRect.Maximize };
        public static readonly ResizeCommand LEFT = new() { HotKeyId = 6, Rect = GridRect.Left };
        public static readonly ResizeCommand TOP = new() { HotKeyId = 7, Rect = GridRect.Top };
        public static readonly ResizeCommand RIGHT = new() { HotKeyId = 8, Rect = GridRect.Right };
        public static readonly ResizeCommand BOTTOM = new() { HotKeyId = 9, Rect = GridRect.Bottom };
    }

    public class ResizeCommand : Command
    {
        public GridRect Rect;

        public override void Execute()
        {
            IntPtr hWnd = WinAPI.GetForegroundWindow();
            if (hWnd == IntPtr.Zero)
            {
                return;
            }

            if (WinAPI.IsZoomed(hWnd))
            {
                WinAPI.ShowWindow(hWnd, WinAPI.SwRestore);
            }

            IntPtr hMonitor = WinAPI.MonitorFromWindow(hWnd, WinAPI.MonitorDefaultToNearest);
            WinAPI.MONITORINFO mi = new WinAPI.MONITORINFO();
            mi.cbSize = (uint)Marshal.SizeOf(typeof(WinAPI.MONITORINFO));
            WinAPI.GetMonitorInfo(hMonitor, ref mi);

            double left = mi.rcWork.Left + mi.rcWork.Width * Rect.X / Rect.GridW;
            double top = mi.rcWork.Top + mi.rcWork.Height * Rect.Y / Rect.GridH;
            double width = mi.rcWork.Width * Rect.W / Rect.GridW;
            double height = mi.rcWork.Height * Rect.H / Rect.GridH;

            WinAPI.RECT clientRect = new()
            {
                Left = (int)left,
                Top = (int)top,
                Right = (int)(left + width),
                Bottom = (int)(top + height)
            };

            uint style = (uint)WinAPI.GetWindowLong(hWnd, WinAPI.GwlStyle);
            uint exStyle = (uint)WinAPI.GetWindowLong(hWnd, WinAPI.GwlExStyle);

            WinAPI.RECT windowRect = clientRect;
            WinAPI.AdjustWindowRectEx(ref windowRect, style, false, exStyle);

            int topMargin = Math.Abs(clientRect.Top - windowRect.Top);

            WinAPI.MoveWindow(
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
