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
using System.Windows.Interop;

namespace Sidebar
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        private SidebarWindow SidebarWindow;
        private IntPtr SidebarHandle;

        public OptionsWindow(SidebarWindow wnd)
        {
            InitializeComponent();
            SidebarWindow = wnd;
            SidebarHandle = new WindowInteropHelper(wnd).Handle;
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
            CopyrightProject.Text = string.Format("© 2016-{0} The AvalonBar Project.",
                DateTime.Now.Year);

            Paragraph block = new Paragraph();
            string licensePath = System.IO.Path.Combine(App.Settings.path, "LICENSE");
            Run licenseMissing = new Run((string)TryFindResource("LicenseFileMissing"));

            block.Inlines.Add(licenseMissing);
            if (File.Exists(licensePath))
            {
                block.Inlines.Remove(licenseMissing);
                block.Inlines.Add(File.ReadAllText(licensePath));
            }

            LicenseTextBox.Document = new FlowDocument(block);
            //-----------------

            AutostartCheckBox.IsChecked = App.Settings.startup;
            TopMostCheckBox.IsChecked = App.Settings.topMost;
            LockedCheckBox.IsChecked = App.Settings.locked;

            if (App.Settings.side == AppBarSide.Left)
                LocationComboBox.SelectedIndex = 0;
            else
                LocationComboBox.SelectedIndex = 1;

            string[] locales = LocaleManager.GetLocales(Sidebar.App.Settings.path);
            for (int i = 0; i <= locales.Length - 1; i++)
            {
                ComboBoxItem item2 = new ComboBoxItem();
                item2.Content = locales[i];
                LangComboBox.Items.Add(item2);
            }
            LangComboBox.Text = App.Settings.locale;

            if (CompositionManager.AvailableCompositionMethod != CompositionMethod.None)
            {
                AeroGlassCheckBox.IsEnabled = true;
                AeroGlassCheckBox.IsChecked = App.Settings.enableGlass;
            }
            else
            {
                AeroGlassCheckBox.IsEnabled = false;
                AeroGlassCheckBox.IsChecked = false;
            }

            if (App.Settings.topMost)
            {
                OverlapCheckBox.IsEnabled = true;
                OverlapCheckBox.IsChecked = App.Settings.overlapTaskbar;
            }
            else
            {
                OverlapCheckBox.IsEnabled = false;
                OverlapCheckBox.IsChecked = false;
            }

            if (App.Settings.enableShadow)
                ShadowCheckBox.IsChecked = true;
            else
                ShadowCheckBox.IsChecked = false;

            UpdatesCheckBox.IsChecked = App.Settings.enableUpdates;

            string[] themes = ThemesManager.GetThemes(Sidebar.App.Settings.path);
            for (int i = 0; i <= themes.Length - 1; i++)
            {
                ComboBoxItem newItem = new ComboBoxItem();
                newItem.Content = themes[i];
                ThemesComboBox.Items.Add(newItem);
            }
            ThemesComboBox.Text = App.Settings.theme;
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
            if (App.Settings.screen == "Primary")
                ScreenComboBox.SelectedIndex = 0;
            else
            {
                ScreenComboBox.SelectedIndex = 0;
                foreach (ComboBoxItem cbItem in ScreenComboBox.Items)
                    if ((string)cbItem.Content != "Primary" && (string)cbItem.Tag == App.Settings.screen)
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
                object enableGlass = ThemesManager.GetThemeParameter(Sidebar.App.Settings.path, ((ComboBoxItem)e.AddedItems[0]).Content.ToString(), "boolean", "EnableGlass");
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
            if (App.Settings.overlapTaskbar && !(bool)OverlapCheckBox.IsChecked)
            {
                AppBar.RestoreTaskbar();
            }

            App.Settings.startup = (bool)AutostartCheckBox.IsChecked;
            App.Settings.topMost = (bool)TopMostCheckBox.IsChecked;
            App.Settings.locked = (bool)LockedCheckBox.IsChecked;
            App.Settings.overlapTaskbar = (bool)OverlapCheckBox.IsChecked;
            App.Settings.enableGlass = (bool)AeroGlassCheckBox.IsChecked;
            App.Settings.enableShadow = (bool)ShadowCheckBox.IsChecked;
            App.Settings.locale = LangComboBox.Text;
            App.Settings.theme = ThemesComboBox.Text;
            App.Settings.enableUpdates = (bool)UpdatesCheckBox.IsChecked;

            if (ScreenComboBox.SelectedIndex == 0)
                App.Settings.screen = "Primary";
            else
                App.Settings.screen = Utils.GetScreenFromFriendlyName(ScreenComboBox.Text).DeviceName;

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
                App.Settings.side = AppBarSide.Left;
            else
                App.Settings.side = AppBarSide.Right;

            if (CompositionManager.AvailableCompositionMethod != CompositionMethod.None && App.Settings.enableGlass)
                CompositionManager.SetBlurBehindWindow(ref SidebarHandle, true);
            else
                CompositionManager.SetBlurBehindWindow(ref SidebarHandle, false);

            if (ShadowCheckBox.IsChecked == true)
                SidebarWindow.shadow.Show();
            else
                SidebarWindow.shadow.Hide();

            AppBar.AppbarRemove();
            SidebarWindow.SetSide(App.Settings.side);

            SidebarWindow.SetTheme(ThemesComboBox.Text);
            SidebarWindow.SetLocale(LangComboBox.Text);

        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ApplySettings();
            ApplyButton.IsEnabled = false;
        }

        private void FindLocalesTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(Services.LanguagesUrl);
        }

        private void FindThemesTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(Services.ThemesUrl);
        }

        private void ContactString_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(Services.LandingPageUrl);
        }

        private void TopMostCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            OverlapCheckBox.IsEnabled = true;
            OverlapCheckBox.IsChecked = App.Settings.overlapTaskbar;
        }

        private void TopMostCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            OverlapCheckBox.IsEnabled = false;
            OverlapCheckBox.IsChecked = false;
        }
    }
}
