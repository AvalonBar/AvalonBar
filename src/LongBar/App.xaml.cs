using System;
using System.Reflection;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Threading;
using System.IO;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.ApplicationServices;

namespace LongBar
{
	/// <summary>
	/// Interaction logic for App.xaml
  	/// </summary>
	public partial class App : Application
	{
    	private static string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    	private static string userPath = LongBarMain.sett.path;
    	
    	public static void HandleError(Exception ex)
        {
    		if (LongBarMain.sett.showErrors) {
        	TaskDialogs.ErrorDialog.ShowDialog((string)Application.Current.TryFindResource("ErrorOccured1"), 
						String.Format("Error: {0}\nSource: {1}\nSee log for detailed info.", 
						ex.Message, ex.Source), ex);
    		}
    	}
    	
    	private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    	{
	        if (!Directory.Exists(userPath + @"\Logs"))
    	        Directory.CreateDirectory(userPath + @"\Logs");
				string logFile = String.Format(@"{0}\Logs\{1}.{2}.{3}.log", userPath, DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);
    	    try
        	{
        	    File.AppendAllText(logFile, String.Format("{0}\r\n{1}\r\n--------------------------------------------------------------------------------------\r\n",
        	        DateTime.UtcNow, e.Exception));
        	}
        	catch (Exception ex)
        	{
        	    MessageBox.Show("Can't write log. Reason: " + ex.Message, null, MessageBoxButton.OK, MessageBoxImage.Asterisk);
        	}
        	HandleError(e.Exception);
        	e.Handled = true;
    	}

		private void App_Startup(object sender, StartupEventArgs e)
    	{
			SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
			Slate.General.SystemTray.isRunning = false;
			if (Slate.Utilities.Utils.PriorProcess() != null)
        	Slate.General.SystemTray.isRunning = true;
      		LongBarMain.ReadSettings();
      		
      		// If the user is using Windows 8 or above.
      		if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 2) {
      			Slate.Themes.ThemesManager.LoadUITheme("/PresentationFramework.AeroLite,Version=4.0.0.0,Culture=neutral,PublicKeyToken=31bf3856ad364e35;component/themes/aerolite.normalcolor.xaml");
      		}
      		// If the user is using Windows Vista or above.
      		if (Environment.OSVersion.Version.Build >= 5600 && Environment.OSVersion.Version.Major >= 6) {
      			Slate.Themes.ThemesManager.LoadUITheme("/PresentationFramework.Aero,Version=3.0.0.0,Culture=neutral,PublicKeyToken=31bf3856ad364e35;component/themes/aero.normalcolor.xaml");
      		}
      		// If the user is using Windows XP or lower.
      		if (Environment.OSVersion.Version.Major <= 5) {
      			Slate.Themes.ThemesManager.LoadUITheme("/PresentationFramework.Luna,Version=3.0.0.0,Culture=neutral,PublicKeyToken=31bf3856ad364e35;component/themes/luna.normalcolor.xaml");
      		}
      		
      		Slate.Localization.LocaleManager.LoadLocale(LongBarMain.sett.path, LongBarMain.sett.locale);

      		// If automatic restart is set to true in settings and if the operating
      		// system is Vista or higher. Code below isn't tested yet in Windows 8 or higher.
      		// If it is proven incompatible, please post this as an issue in the tracker
      		// in the Github repo.
      		if (Slate.Data.XMLReader.ReadXML("Experimental", "AllowAutomaticRestart", Slate.Data.XMLReader.SettingsLoc) == "true" && Environment.OSVersion.Version.Major >= 6) {
      			// Code below is based on the sample found in the WinApiCodePack.
      		    // Register for automatic restart if the application was terminated for any reason
            	// other than a system reboot or a system update.
            	ApplicationRestartRecoveryManager.RegisterForApplicationRestart(
                	new RestartSettings("/restart", RestartRestrictions.NotOnReboot | RestartRestrictions.NotOnPatch));
      		}
      		
		    if (Slate.General.SystemTray.isRunning && e.Args.Length == 0)
      		{
          		MessageBox.Show((string)Application.Current.TryFindResource("AlreadyRunning"), (string)Application.Current.TryFindResource("ApplicationName"), System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
  		        Shutdown();
      		}
		    
		    if (e.Args.Length > 0)
		    {
				switch (e.Args[0])
				{
					case @"/regext":
						try 
						{
							RegistryKey key;
							key = Registry.ClassesRoot;
              				key = key.CreateSubKey(".tile", RegistryKeyPermissionCheck.ReadWriteSubTree);
              				key.SetValue(null, "HornSide.Tile", RegistryValueKind.String);
              				key = Registry.ClassesRoot;
              				key = key.CreateSubKey("HornSide.Tile", RegistryKeyPermissionCheck.ReadWriteSubTree);
              				key.SetValue(null, "HornSide Tile", RegistryValueKind.String);
              				key = key.CreateSubKey("DefaultIcon", RegistryKeyPermissionCheck.ReadWriteSubTree);
              				key.SetValue(null, path + @"\Core.dll,0", RegistryValueKind.ExpandString);
              				key = Registry.ClassesRoot;
              				key = key.OpenSubKey("HornSide.Tile", RegistryKeyPermissionCheck.ReadWriteSubTree);
              				key = key.CreateSubKey("shell", RegistryKeyPermissionCheck.ReadWriteSubTree);
              				key = key.CreateSubKey("Install", RegistryKeyPermissionCheck.ReadWriteSubTree);
              				key = key.CreateSubKey("command", RegistryKeyPermissionCheck.ReadWriteSubTree);
              				key.SetValue(null, path + @"\" + Assembly.GetExecutingAssembly().GetName().Name + @".exe %1", RegistryValueKind.String);
              				key.Close();
            			}
						catch (Exception ex) { HandleError(ex); }
					break;

					case @"/unregext":
	            		try
	            		{
	              			RegistryKey key;
	              			key = Registry.ClassesRoot.OpenSubKey(".tile", RegistryKeyPermissionCheck.ReadWriteSubTree);
	              			if (key != null)
	                		Registry.ClassesRoot.DeleteSubKeyTree(".tile");
	              			key = Registry.ClassesRoot.OpenSubKey("HornSide.Tile", RegistryKeyPermissionCheck.ReadWriteSubTree);
	              			if (key != null)
	                		Registry.ClassesRoot.DeleteSubKeyTree("HornSide.Tile");
	              			key.Close();
	            		}
	            		catch (Exception ex) { HandleError(ex); }
            		break;

            		case "/debug":
	            		if (e.Args.Length > 1 && e.Args[1].EndsWith(".dll") && File.Exists(e.Args[1]))
	            		{
	                		LongBarMain.sett.debug = true;
	  		                LongBarMain.sett.tileToDebug = e.Args[1];
	            		}
            		break;
                    
          			default:
	            		foreach (string file in e.Args)
	            		{
	               			try
	                		{
	                    		string longFile = Path.GetFullPath(file);
	                    		if (File.Exists(longFile) && Path.GetExtension(longFile) == ".tile")
	                    		{
	                        		FileInfo info = new FileInfo(longFile);
	                        		TaskDialogs.TileInstallDialog.ShowDialog(null, info.Name, longFile);
	                    		}
	                		}
	               			catch (Exception ex) { HandleError(ex); }
	            		}
		            break;
				}
        		if (Slate.General.SystemTray.isRunning)
            	Shutdown();
		    }
    	}

		private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
		{
			Slate.General.Sidebar.ResizeBar();
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
		}
	}
}