using System.Windows;
using System.Windows.Forms;

namespace window_layout_manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private NotifyIcon? _norifyIcon;
        private HotKeyService? _hotKeyService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _hotKeyService = new HotKeyService();
            _hotKeyService.Start();

            _norifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application,
                Visible = true,
                Text = "Window Layout Manager"
            };

            var menu = new ContextMenuStrip();
            menu.Items.Add("Option", null, (s, ev) =>
            {
                var win = new MainWindow();
                win.Show();
            });

            menu.Items.Add("Exit", null, (s, ev) =>
            {
                _norifyIcon.Visible = false;
                Shutdown();
            });

            _norifyIcon.ContextMenuStrip = menu;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _hotKeyService?.Dispose();

            base.OnExit(e);
        }
    }
}
