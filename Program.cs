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

            var preference = new PreferenceRepository();
            using var trayIcon = new TrayIcon(preference);
            trayIcon.ExitMenuClicked += (s, e) => Application.Exit();

            using var hotKeyService = new HotKeyService(preference);

            //var preferenceWindow = new PreferenceWindow();

            Application.Run();
        }
    }
}