namespace window_layout_manager
{
    public struct GridRect
    {
        public double GridW;
        public double GridH;
        public double X;
        public double Y;
        public double W;
        public double H;

        public static GridRect TopLeft = new() { GridW = 2, GridH = 2, X = 0, Y = 0, W = 1, H = 1 };
        public static GridRect TopRight = new() { GridW = 2, GridH = 2, X = 1, Y = 0, W = 1, H = 1 };
        public static GridRect BottomLeft = new() { GridW = 2, GridH = 2, X = 0, Y = 1, W = 1, H = 1 };
        public static GridRect BottomRight = new() { GridW = 2, GridH = 2, X = 1, Y = 1, W = 1, H = 1 };
        public static GridRect Maximize = new() { GridW = 1, GridH = 1, X = 0, Y = 0, W = 1, H = 1 };
        public static GridRect Left = new() { GridW = 2, GridH = 1, X = 0, Y = 0, W = 1, H = 1 };
        public static GridRect Right = new() { GridW = 2, GridH = 1, X = 1, Y = 0, W = 1, H = 1 };
        public static GridRect Top = new() { GridW = 1, GridH = 2, X = 0, Y = 0, W = 1, H = 1 };
        public static GridRect Bottom = new() { GridW = 1, GridH = 2, X = 0, Y = 1, W = 1, H = 1 };
    }
}