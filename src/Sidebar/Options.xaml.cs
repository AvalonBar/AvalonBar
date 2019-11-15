using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using Sidebar.Core;

namespace Sidebar
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        private SidebarWindow longBar;

        public Options(SidebarWindow wnd)
        {
            InitializeComponent();
            longBar = wnd;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //AboutWindow.xaml
            VersionString.Text = string.Format("{0} {1} ({2})",
                TryFindResource("Version"), VersionInfo.Core, VersionInfo.Configuration);
            CopyrightLongBar.Text = string.Format("© 2010 LongBar Project Group. {0}",
                TryFindResource("AllRightsReserved"));
            CopyrightProject.Text = string.Format("© 2016-{1} The AvalonBar Project. {0}",
                TryFindResource("AllRightsReserved"), DateTime.Now.Year);
            //-----------------

            AutostartCheckBox.IsChecked = SidebarWindow.sett.startup;
            TopMostCheckBox.IsChecked = SidebarWindow.sett.topMost;
            LockedCheckBox.IsChecked = SidebarWindow.sett.locked;

            if (SidebarWindow.sett.side == AppBarSide.Left)
                LocationComboBox.SelectedIndex = 0;
            else
                LocationComboBox.SelectedIndex = 1;

            string[] locales = LocaleManager.GetLocales(Sidebar.SidebarWindow.sett.path);
            for (int i = 0; i <= locales.Length - 1; i++)
            {
                ComboBoxItem item2 = new ComboBoxItem();
                item2.Content = locales[i];
                LangComboBox.Items.Add(item2);
            }
            LangComboBox.Text = SidebarWindow.sett.locale;

            if (DwmManager.IsBlurAvailable)
            {
                AeroGlassCheckBox.IsEnabled = true;
                AeroGlassCheckBox.IsChecked = SidebarWindow.sett.enableGlass;
            }
            else
            {
                AeroGlassCheckBox.IsEnabled = false;
                AeroGlassCheckBox.IsChecked = false;
            }

            if (SidebarWindow.sett.topMost)
            {
                OverlapCheckBox.IsEnabled = true;
                OverlapCheckBox.IsChecked = SidebarWindow.sett.overlapTaskbar;
            }
            else
            {
                OverlapCheckBox.IsEnabled = false;
                OverlapCheckBox.IsChecked = false;
            }

            if (SidebarWindow.sett.enableShadow)
                ShadowCheckBox.IsChecked = true;
            else
                ShadowCheckBox.IsChecked = false;

            UpdatesCheckBox.IsChecked = SidebarWindow.sett.enableUpdates;

            string[] themes = ThemesManager.GetThemes(Sidebar.SidebarWindow.sett.path);
            for (int i = 0; i <= themes.Length - 1; i++)
            {
                ComboBoxItem newItem = new ComboBoxItem();
                newItem.Content = themes[i];
                ThemesComboBox.Items.Add(newItem);
            }
            ThemesComboBox.Text = SidebarWindow.sett.theme;
            ApplyButton.IsEnabled = false;

            string[] screenNames = Utils.GetScreenFriendlyNames();
            System.Windows.Forms.Screen[] screens = System.Windows.Forms.Screen.AllScreens;
            for (int i = 0; i < screenNames.Length; i++)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = screenNames[i];
                item.Tag = screens[i].DeviceName;
                ScreenComboBox.Items.Add(item);
            }
            if (SidebarWindow.sett.screen == "Primary")
                ScreenComboBox.SelectedIndex = 0;
            else
            {
                ScreenComboBox.SelectedIndex = 0;
                foreach (ComboBoxItem cbItem in ScreenComboBox.Items)
                    if ((string)cbItem.Content != "Primary" && (string)cbItem.Tag == SidebarWindow.sett.screen)
                    {
                        ScreenComboBox.SelectedItem = cbItem;
                        break;
                    }
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            ApplyButton.IsEnabled = true;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyButton.IsEnabled = true;
            if (ThemesComboBox != null && ThemesComboBox.SelectionBoxItem != null)
            {
                object enableGlass = ThemesManager.GetThemeParameter(Sidebar.SidebarWindow.sett.path, ((ComboBoxItem)e.AddedItems[0]).Content.ToString(), "boolean", "EnableGlass");
                if (enableGlass != null)
                {
                    AeroGlassCheckBox.IsChecked = Convert.ToBoolean(enableGlass);
                }
            }
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            ApplyButton.IsEnabled = true;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (ApplyButton.IsEnabled)
            {
                ApplySettings();
                ApplyButton.IsEnabled = false;
            }
            this.Close();
        }

        private void ApplySettings()
        {
            if (SidebarWindow.sett.overlapTaskbar && !(bool)OverlapCheckBox.IsChecked)
            {
                AppBar.RestoreTaskbar();
            }

            SidebarWindow.sett.startup = (bool)AutostartCheckBox.IsChecked;
            SidebarWindow.sett.topMost = (bool)TopMostCheckBox.IsChecked;
            SidebarWindow.sett.locked = (bool)LockedCheckBox.IsChecked;
            SidebarWindow.sett.overlapTaskbar = (bool)OverlapCheckBox.IsChecked;
            SidebarWindow.sett.enableGlass = (bool)AeroGlassCheckBox.IsChecked;
            SidebarWindow.sett.enableShadow = (bool)ShadowCheckBox.IsChecked;
            SidebarWindow.sett.locale = LangComboBox.Text;
            SidebarWindow.sett.theme = ThemesComboBox.Text;
            SidebarWindow.sett.enableUpdates = (bool)UpdatesCheckBox.IsChecked;

            if (ScreenComboBox.SelectedIndex == 0)
                SidebarWindow.sett.screen = "Primary";
            else
                SidebarWindow.sett.screen = Utils.GetScreenFromFriendlyName(ScreenComboBox.Text).DeviceName;

            if ((bool)AutostartCheckBox.IsChecked)
            {
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree).OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("Run", true))
                    {
                        key.SetValue("LongBar", "" + Assembly.GetExecutingAssembly().Location + "", RegistryValueKind.String);
                        key.Close();
                    }
                }
                catch { }
            }
            else
            {
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree).OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("Run", true))
                    {
                        key.DeleteValue("LongBar", false);
                        key.Close();
                    }
                }
                catch { }
            }

            if (LocationComboBox.SelectedIndex == 0)
                SidebarWindow.sett.side = AppBarSide.Left;
            else
                SidebarWindow.sett.side = AppBarSide.Right;

            if (DwmManager.IsBlurAvailable && SidebarWindow.sett.enableGlass)
                DwmManager.EnableBlurBehindWindow(ref longBar.Handle);
            else
                DwmManager.DisableBlurBehindWindow(ref longBar.Handle);

            if (ShadowCheckBox.IsChecked == true)
                longBar.shadow.Show();
            else
                longBar.shadow.Hide();

            AppBar.AppbarRemove();
            longBar.SetSide(SidebarWindow.sett.side);

            longBar.SetTheme(ThemesComboBox.Text);
            longBar.SetLocale(LangComboBox.Text);

        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ApplySettings();
            ApplyButton.IsEnabled = false;
        }

        private void FindLocalesTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(ServiceUrls.Languages);
        }

        private void FindThemesTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(ServiceUrls.Themes);
        }

        private void ContactString_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(ServiceUrls.LandingPage);
        }

        private void TopMostCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            OverlapCheckBox.IsEnabled = true;
            OverlapCheckBox.IsChecked = SidebarWindow.sett.overlapTaskbar;
        }

        private void TopMostCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            OverlapCheckBox.IsEnabled = false;
            OverlapCheckBox.IsChecked = false;
        }
    }
}
