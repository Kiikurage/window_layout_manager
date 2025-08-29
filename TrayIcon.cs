using System.Windows.Forms;

namespace window_layout_manager
{
    public partial class TrayIcon : IDisposable
    {
        private readonly NotifyIcon notifyIcon;
        private readonly PreferenceRepository preferenceRepository;

        public event EventHandler? ExitMenuClicked;

        public TrayIcon(PreferenceRepository preferenceRepository)
        {
            this.preferenceRepository = preferenceRepository;

            notifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application,
                Visible = true,
                Text = "Window Layout Manager"
            };

            var menu = new ContextMenuStrip();

            var autoStartMenuItem = menu.Items.Add("PC起動時に自動起動", null, (s, ev) =>
            {
                try
                {
                    if (preferenceRepository.IsAutoStartEnabled())
                    {
                        preferenceRepository.DisableAutoStart();
                        (s as ToolStripMenuItem)!.Checked = false;
                    }
                    else
                    {
                        preferenceRepository.EnableAutoStart();
                        (s as ToolStripMenuItem)!.Checked = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"自動起動の設定に失敗しました。\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    (s as ToolStripMenuItem)!.Checked = preferenceRepository.IsAutoStartEnabled();
                }
            }) as ToolStripMenuItem;
            autoStartMenuItem.Checked = preferenceRepository.IsAutoStartEnabled();

            menu.Items.Add("終了", null, (s, ev) =>
            {
                notifyIcon.Visible = false;
                ExitMenuClicked?.Invoke(this, EventArgs.Empty);
            });
            notifyIcon.ContextMenuStrip = menu;
        }

        public void Dispose()
        {
            notifyIcon.Dispose();
        }
    }
}
