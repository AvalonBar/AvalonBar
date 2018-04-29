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
		private static string userPath = LongBarMain.sett.Program.Path;

		public static void HandleError(Exception ex)
		{
			if (LongBarMain.sett.Program.ShowErrors) {
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
			// Close when WinXP is detected
			if (Environment.OSVersion.Version.Major <= 5) {
				MessageBox.Show("Windows XP and lower are not supported by AvalonBar.\nPlease use Windows Vista or higher.", "Unsupported OS", MessageBoxButton.OK, MessageBoxImage.Stop);
				Shutdown();
			}

			SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
			Slate.General.SystemTray.isRunning = false;
			if (Slate.Utilities.Utils.PriorProcess() != null)
			Slate.General.SystemTray.isRunning = true;

			LongBarMain.ReadSettings();

			Slate.Localization.LocaleManager.LoadLocale(LongBarMain.sett.Program.Path, LongBarMain.sett.Program.Language);

			// Register for automatic restart if the application was terminated for any reason
			// other than a system reboot or a system update.
			ApplicationRestartRecoveryManager.RegisterForApplicationRestart(new RestartSettings("/restart", RestartRestrictions.NotOnReboot | RestartRestrictions.NotOnPatch));
			
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
							key.SetValue(null, "AvalonBar.Tile", RegistryValueKind.String);
							key = Registry.ClassesRoot;
							key = key.CreateSubKey("AvalonBar.Tile", RegistryKeyPermissionCheck.ReadWriteSubTree);
							key.SetValue(null, "AvalonBar Tile", RegistryValueKind.String);
							key = key.CreateSubKey("DefaultIcon", RegistryKeyPermissionCheck.ReadWriteSubTree);
							key.SetValue(null, path + @"\Core.dll,0", RegistryValueKind.ExpandString);
							key = Registry.ClassesRoot;
							key = key.OpenSubKey("AvalonBar.Tile", RegistryKeyPermissionCheck.ReadWriteSubTree);
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
	              			key = Registry.ClassesRoot.OpenSubKey("AvalonBar.Tile", RegistryKeyPermissionCheck.ReadWriteSubTree);
	              			if (key != null)
	                		Registry.ClassesRoot.DeleteSubKeyTree("AvalonBar.Tile");
	              			key.Close();
	            		}
	            		catch (Exception ex) { HandleError(ex); }
					break;

					case "/debug":
	            		if (e.Args.Length > 1 && e.Args[1].EndsWith(".dll") && File.Exists(e.Args[1]))
	            		{
	                		LongBarMain.flags.Debug = true;
	  		                LongBarMain.flags.TileToDebug = e.Args[1];
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