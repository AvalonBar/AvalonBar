using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows;
using System.Drawing;
using System.IO;

namespace Sidebar
{
    public class SystemTray : IDisposable
    {
        private NotifyIcon trayIcon;
        private System.Windows.Controls.ContextMenu trayMenu;
        private Window window;
        private System.Windows.Controls.MenuItem closeMenuItem;
        private System.Windows.Controls.MenuItem showHideMenuItem;

        public SystemTray(Window parentWindow)
        {
            trayMenu = new System.Windows.Controls.ContextMenu();

            closeMenuItem = new System.Windows.Controls.MenuItem();
            closeMenuItem.Click += CloseMenuItem_Click;

            showHideMenuItem = new System.Windows.Controls.MenuItem();
            showHideMenuItem.Click += ShowHideMenuItem_Click;

            SetLocale();

            trayMenu.Items.Add(showHideMenuItem);
            trayMenu.Items.Add(closeMenuItem);

            Uri iconUri = new Uri("pack://application:,,,/Sidebar;component/Resources/SidebarIcon.ico");
            Stream iconStream = System.Windows.Application.GetResourceStream(iconUri).Stream;

            trayIcon = new NotifyIcon();
            trayIcon.Icon = new Icon(iconStream);
            trayIcon.Text = "AvalonBar";
            trayIcon.MouseClick += new MouseEventHandler(trayIcon_MouseClick);
            trayIcon.MouseDoubleClick += new MouseEventHandler(trayIcon_MouseDoubleClick);
            trayIcon.Visible = true;

            iconStream.Dispose();

            window = parentWindow;
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ShowHideMenuItem_Click(sender, null);
            }
        }

        private void trayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                window.Activate();
            else
                trayMenu.IsOpen = true;
        }

        public void SetLocale()
        {
            closeMenuItem.Header = Utils.FindString("Close");
            showHideMenuItem.Header = Utils.FindString("ShowHide");
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            window.Close();
        }

        private void ShowHideMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (window.IsVisible)
            {
                window.Hide();
                return;
            }
            window.Show();
        }

        public void Dispose()
        {
            trayIcon.Dispose();
        }
    }
}
