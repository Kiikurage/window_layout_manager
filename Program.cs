using System.Windows.Forms;

namespace window_layout_manager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using var hotKeyService = new HotKeyService();
            Application.Run();
        }
    }
}