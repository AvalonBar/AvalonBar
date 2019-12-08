using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows;
using System.Drawing;
using System.IO;

namespace Sidebar.Core
{
    public class SystemTray
    {
        private static NotifyIcon trayIcon;
        private static System.Windows.Controls.ContextMenu trayMenu;
        private static Window window;
        private static System.Windows.Controls.MenuItem closeMenuItem;
        private static System.Windows.Controls.MenuItem showHideMenuItem;

        public static void AddIcon(Window wnd)
        {
            trayMenu = new System.Windows.Controls.ContextMenu();

            closeMenuItem = new System.Windows.Controls.MenuItem();
            closeMenuItem.Click += CloseMenuItem_Click;

            showHideMenuItem = new System.Windows.Controls.MenuItem();
            showHideMenuItem.Click += ShowHideMenuItem_Click;

            SetLocale();

            trayMenu.Items.Add(showHideMenuItem);
            trayMenu.Items.Add(closeMenuItem);

            Uri iconUri = new Uri("pack://application:,,,/Sidebar;component/SidebarIcon.ico");
            Stream iconStream = System.Windows.Application.GetResourceStream(iconUri).Stream;

            trayIcon = new NotifyIcon();
            trayIcon.Icon = new Icon(iconStream);
            trayIcon.Text = "AvalonBar";
            trayIcon.MouseClick += new MouseEventHandler(trayIcon_MouseClick);
            trayIcon.MouseDoubleClick += new MouseEventHandler(trayIcon_MouseDoubleClick);
            trayIcon.Visible = true;

            window = wnd;
        }

        private static void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ShowHideMenuItem_Click(sender, null);
            }
        }

        private static void trayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                window.Activate();
            else
                trayMenu.IsOpen = true;
        }

        public static void SetLocale()
        {
            closeMenuItem.Header = System.Windows.Application.Current.TryFindResource("Close");
            showHideMenuItem.Header = System.Windows.Application.Current.TryFindResource("ShowHide");
        }

        public static void RemoveIcon()
        {
            trayIcon.MouseClick -= new MouseEventHandler(trayIcon_MouseClick);
            trayIcon.Visible = false;
            trayIcon.Dispose();
        }

        private static void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            window.Close();
        }

        private static void ShowHideMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (window.IsVisible)
            {
                window.Hide();
                return;
            }
            window.Show();
        }
    }
}
