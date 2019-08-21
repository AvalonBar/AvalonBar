using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows;
using System.Drawing;

namespace Sidebar.Core
{
    public class SystemTray
    {
        public delegate void SidebarvisibleChangedEventHandler(bool value);
        public static event SidebarvisibleChangedEventHandler SidebarvisibleChanged;
        private static bool _SidebarVisible = true;
        public static bool isRunning = false;
        protected static void OnSidebarVisibleChanged(bool value)
        {
            if (SidebarvisibleChanged != null)
                SidebarvisibleChanged(value);
        }

        [DllImport("shell32.dll")]
        private static extern IntPtr ExtractIcon(IntPtr hInstance, string path, int iconIndex);
        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern int SetForeGroundWindow(ref IntPtr hWnd);

        private static string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        private static NotifyIcon trayIcon;
        private static System.Windows.Controls.ContextMenu trayMenu;
        private static Window window;
        private static System.Windows.Controls.MenuItem closeMenuItem;
        private static System.Windows.Controls.MenuItem showHideMenuItem;

        public static void AddIcon(Window wnd)
        {
            if (isRunning)
                return;
            trayMenu = new System.Windows.Controls.ContextMenu();

            closeMenuItem = new System.Windows.Controls.MenuItem();
            closeMenuItem.AddHandler(System.Windows.Controls.MenuItem.ClickEvent, new RoutedEventHandler(CloseMenuItem_Click));

            showHideMenuItem = new System.Windows.Controls.MenuItem();
            showHideMenuItem.AddHandler(System.Windows.Controls.MenuItem.ClickEvent, new RoutedEventHandler(ShowHideMenuItem_Click));

            SetLocale();

            trayMenu.Items.Add(showHideMenuItem);
            trayMenu.Items.Add(closeMenuItem);

            trayIcon = new NotifyIcon();
            // FIXME: application executable name should not be hardcoded
            trayIcon.Icon = Icon.FromHandle(ExtractIcon(IntPtr.Zero, path + @"\Sidebar.exe", 0));
            trayIcon.Text = "AvalonBar";
            trayIcon.MouseClick += new MouseEventHandler(trayIcon_MouseClick);
            trayIcon.MouseDoubleClick += new MouseEventHandler(trayIcon_MouseDoubleClick);
            trayIcon.Visible = true;
            window = wnd;
        }

        private static void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                if (!SidebarVisible)
                    SystemTray.SidebarVisible = true;
                else SystemTray.SidebarVisible = false;
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
            if (isRunning)
                return;
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
            if (!SidebarVisible)
                SystemTray.SidebarVisible = true;
            else SystemTray.SidebarVisible = false;
        }

        private static bool overlapTaskbar = false;

        public static bool SidebarVisible
        {
            get
            {
                return _SidebarVisible;
            }
            set
            {
                if (value)
                {
                    ShowWindow(Appbar.Handle, 5);
                    if (Appbar.AlwaysTop)
                    {
                        Appbar.AppbarRemove();
                        Appbar.AppbarNew();
                        if (!Appbar.Overlapped && overlapTaskbar)
                            Appbar.OverlapTaskbar();
                        Appbar.SizeAppbar();
                    }
                    _SidebarVisible = true;
                    OnSidebarVisibleChanged(true);
                }
                else
                {
                    ShowWindow(Appbar.Handle, 0);
                    if (Appbar.AlwaysTop)
                    {
                        Appbar.AppbarRemove();
                        if (Appbar.Overlapped)
                        {
                            Appbar.UnOverlapTaskbar();
                            overlapTaskbar = true;
                        }
                        else
                            overlapTaskbar = true;
                    }
                    _SidebarVisible = false;
                    OnSidebarVisibleChanged(false);
                }
            }
        }
    }
}
