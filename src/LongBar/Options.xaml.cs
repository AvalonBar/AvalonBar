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
using Microsoft.WindowsAPICodePack.Shell;

namespace LongBar
{
  /// <summary>
  /// Interaction logic for Options.xaml
  /// </summary>
  public partial class Options : GlassWindow
  {
    private LongBarMain longBar;
    
    public Options(LongBarMain wnd)
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
        Assembly _AsmObj = Assembly.GetExecutingAssembly();
        AssemblyName _CurrAsmName = _AsmObj.GetName();
        string _Major = _CurrAsmName.Version.Major.ToString();
        string _Minor = _CurrAsmName.Version.Minor.ToString();
        string _Build = _CurrAsmName.Version.Build.ToString();//.Substring(0, 1);
        string _Revision = _CurrAsmName.Version.Revision.ToString();//.Substring(0, 1);
		
        ProjectVer2.Text = AssemblyInfo.SharedIV + " " + GitInfo.BranchProdStatus;
        ProjectMilestone.Text = GitInfo.Milestone;
        ProjectName.Text = string.Format("{0} {1}", GitInfo.Repository, GitInfo.Milestone);
        VersionString.Text = string.Format("{0} {2}.{3} {6}. {1} {4} (L{2}.{3}.{4}.{5}b)",
        TryFindResource("Version"), TryFindResource("Build"), _Major, _Minor, _Build, _Revision, GitInfo.BranchProdStatus);
        CurrentMaintainer.Text = "Portions © The AvalonBar Project " + DateTime.Today.Year;
        OldMaintainer.Text = "Portions © LongBar Project Group 2010.";
        CopyrightString2.Text = String.Format("{0}", Application.Current.TryFindResource("CopyrightLaw"));
        //-----------------

        AutostartCheckBox.IsChecked = LongBarMain.sett.startup;
        TopMostCheckBox.IsChecked = LongBarMain.sett.topMost;
        LockedCheckBox.IsChecked = LongBarMain.sett.locked;
		// Glass region
			// update background color on change of desktop composition mode
            AeroGlassCompositionChanged += delegate(object wsender, AeroGlassCompositionChangedEventArgs we) {
            	// When the desktop composition mode changes the background color
	            // and window exclusion must be changed appropriately.
	            if (Slate.DWM.DwmManager.IsGlassAvailable()) //we.GlassAvailable)
	            {
	                SetAeroGlassTransparency();
	                InvalidateVisual();
	            }
	            else
	            {
	            	Background = Brushes.White;
	            }            	
            };

            // Set the window background color
            if (AeroGlassCompositionEnabled)
            {
            	if (Slate.DWM.DwmManager.IsGlassAvailable()) {
            		SetAeroGlassTransparency();
            	}
            }
            else
            {
                Background = Brushes.White;
            }
        if (LongBarMain.sett.side == Slate.General.Sidebar.Side.Left)
            LocationComboBox.SelectedIndex = 0;
        else
            LocationComboBox.SelectedIndex = 1;

        string[] locales = Slate.Localization.LocaleManager.GetLocales(LongBar.LongBarMain.sett.path);
        for (int i = 0; i <= locales.Length - 1; i++)
        {
            ComboBoxItem item2 = new ComboBoxItem();
            item2.Content = locales[i];
            LangComboBox.Items.Add(item2);
        }
        LangComboBox.Text = LongBarMain.sett.locale;

        if (Slate.DWM.DwmManager.IsGlassAvailable())
        {
            AeroGlassCheckBox.IsEnabled = true;
            AeroGlassCheckBox.IsChecked = LongBarMain.sett.enableGlass;
        }
        else
        {
            AeroGlassCheckBox.IsEnabled = false;
            AeroGlassCheckBox.IsChecked = false;
        }

        if (LongBarMain.sett.topMost)
        {
            OverlapCheckBox.IsEnabled = true;
            OverlapCheckBox.IsChecked = LongBarMain.sett.overlapTaskbar;
        }
        else
        {
            OverlapCheckBox.IsEnabled = false;
            OverlapCheckBox.IsChecked = false;
        }

        if (LongBarMain.sett.enableShadow)
            ShadowCheckBox.IsChecked = true;
        else
            ShadowCheckBox.IsChecked = false;
        
        UpdatesCheckBox.IsChecked = LongBarMain.sett.enableUpdates;

        string[] themes = Slate.Themes.ThemesManager.GetThemes(LongBar.LongBarMain.sett.path);
        for (int i = 0; i <= themes.Length - 1; i++)
        {
            ComboBoxItem newItem = new ComboBoxItem();
            newItem.Content = themes[i];
            ThemesComboBox.Items.Add(newItem);
        }
        ThemesComboBox.Text = LongBarMain.sett.theme;
		ApplyButton.IsEnabled = false;

        string[] screenNames = Slate.Utilities.Utils.GetScreenFriendlyNames();
        System.Windows.Forms.Screen[] screens = System.Windows.Forms.Screen.AllScreens;
        for (int i = 0; i < screenNames.Length; i++)
        {
          ComboBoxItem item = new ComboBoxItem();
          item.Content = screenNames[i];
          item.Tag = screens[i].DeviceName;
          ScreenComboBox.Items.Add(item);
        }
        if (LongBarMain.sett.screen == "Primary")
          ScreenComboBox.SelectedIndex = 0;
        else
        {
          ScreenComboBox.SelectedIndex = 0;
          foreach (ComboBoxItem cbItem in ScreenComboBox.Items)
            if ((string)cbItem.Content!="Primary" && (string)cbItem.Tag == LongBarMain.sett.screen)
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
            object enableGlass = Slate.Themes.ThemesManager.GetThemeParameter(LongBar.LongBarMain.sett.path, ((ComboBoxItem)e.AddedItems[0]).Content.ToString(), "boolean", "EnableGlass");
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
        Close();
    }

    private void ApplySettings()
    {
        if (LongBarMain.sett.overlapTaskbar && !(bool)OverlapCheckBox.IsChecked)
        {
            Slate.General.Sidebar.UnOverlapTaskbar();
        }

        LongBarMain.sett.startup = (bool)AutostartCheckBox.IsChecked;
        LongBarMain.sett.topMost = (bool)TopMostCheckBox.IsChecked;
        LongBarMain.sett.locked = (bool)LockedCheckBox.IsChecked;
        LongBarMain.sett.overlapTaskbar = (bool)OverlapCheckBox.IsChecked;
        LongBarMain.sett.enableGlass = (bool)AeroGlassCheckBox.IsChecked;
        LongBarMain.sett.enableShadow = (bool)ShadowCheckBox.IsChecked;
        LongBarMain.sett.locale = LangComboBox.Text;
        LongBarMain.sett.theme = ThemesComboBox.Text;
        LongBarMain.sett.enableUpdates = (bool)UpdatesCheckBox.IsChecked;

        if (LongBarMain.sett.enableSnowFall)
            longBar.EnableSnowFall();
        else
            longBar.DisableSnowFall();

        if (ScreenComboBox.SelectedIndex == 0)
          LongBarMain.sett.screen = "Primary";
        else
          LongBarMain.sett.screen = Slate.Utilities.Utils.GetScreenFromFriendlyName(ScreenComboBox.Text).DeviceName;

        if ((bool)AutostartCheckBox.IsChecked)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree).OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("Run", true))
                {
                    key.SetValue("HornSide", "" + Assembly.GetExecutingAssembly().Location + "", RegistryValueKind.String);
                    key.Close();
                }
            }
            catch (Exception ex) { App.HandleError(ex); }
        }
        else
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree).OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("Run", true))
                {
                    key.DeleteValue("HornSide",false);
                    key.Close();
                }
            }
            catch (Exception ex) { App.HandleError(ex); }
        }

        if (LocationComboBox.SelectedIndex == 0)
            LongBarMain.sett.side = Slate.General.Sidebar.Side.Left;
        else
            LongBarMain.sett.side = Slate.General.Sidebar.Side.Right;

        if (Environment.OSVersion.Version.Major >= 6)
        {
            if (Slate.DWM.DwmManager.IsGlassAvailable() && LongBarMain.sett.enableGlass)
                Slate.DWM.DwmManager.EnableGlass(ref longBar.Handle, IntPtr.Zero);
            else
                Slate.DWM.DwmManager.DisableGlass(ref longBar.Handle);
        }

        if (ShadowCheckBox.IsChecked == true)
            longBar.shadow.Show();
        else
            longBar.shadow.Hide();

        Slate.General.Sidebar.AppbarRemove();

        longBar.SetSide(LongBarMain.sett.side);
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
        Process.Start("http://cid-820d4d5cef8566bf.skydrive.live.com/browse.aspx/LongBar%20Project/Localization%202.0");
    }

    private void FindThemesTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Process.Start("http://cid-820d4d5cef8566bf.skydrive.live.com/browse.aspx/LongBar%20Project/Themes%202.0");
    }

    private void ContactString_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Process.Start(Slate.Data.XMLReader.ReadSettings("Links", "ProjectURL"));
    }

    private void TopMostCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        OverlapCheckBox.IsEnabled = true;
        OverlapCheckBox.IsChecked = LongBarMain.sett.overlapTaskbar;
    }

    private void TopMostCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        OverlapCheckBox.IsEnabled = false;
        OverlapCheckBox.IsChecked = false;
    }

    private void UnAllTilesButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
    	//TODO: Localize stuff below [1]
    	switch (MessageBox.Show("Are you sure you want to uninstall all tiles?", "HornSide", MessageBoxButton.YesNo, MessageBoxImage.Question))
    	{
    		case MessageBoxResult.Yes:
    			MessageBox.Show("failed");
    			break;
    		case MessageBoxResult.No:
    			break;
    		case MessageBoxResult.None:
    			break;
    	}
    }

    private void ReportString_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
		Process.Start(Slate.Data.XMLReader.ReadSettings("Links", "BugTrackerURL"));
	}
  }
}
