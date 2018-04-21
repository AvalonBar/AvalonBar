using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAPICodePack;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Win32;
using System.Windows;
using System.IO;
using System.Windows.Controls;

namespace LongBar.TaskDialogs
{
	public class TileInstallDialog
	{
		private static TaskDialog td = null;
		private static TaskDialog tdResult = null;

		private static string tilePath;
		private static string tileName;

		private static LongBarMain longBar;

		public static void ShowDialog(LongBarMain longbar, string name, string path)
		{
			longBar = longbar;

			tilePath = path;
			tileName = name;
			if (Environment.OSVersion.Version.Major >= 6)
			{
					td = new TaskDialog();
					td.Cancelable = true;
					td.Icon = TaskDialogStandardIcon.None;

					td.Caption = (string)Application.Current.TryFindResource("InstallingTile");
					td.Text = path;
					td.InstructionText = string.Format((string)Application.Current.TryFindResource("Dontdoit"), tileName);
					td.StandardButtons = TaskDialogStandardButtons.Cancel;

					TaskDialogCommandLink installButton = new TaskDialogCommandLink("installButton", (string)Application.Current.TryFindResource("InstallThisIncredibleTileForMePlease1"), (string)Application.Current.TryFindResource("InstallThisIncredibleTileForMePlease2"));
					installButton.Click += new EventHandler(installButton_Click);

					td.Controls.Add(installButton);
					td.Show();
			}
			else
			{
				if (System.Windows.MessageBox.Show(string.Format((string)Application.Current.TryFindResource("Dontdoit"), tileName), (string)Application.Current.TryFindResource("InstallingTile"), System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Information) == System.Windows.MessageBoxResult.Yes)
				{
					try
					{
						Slate.Packaging.PackageManager.Unpack(LongBar.LongBarMain.sett.path, tilePath);
						System.Windows.MessageBox.Show(tileName + " " + (string)Application.Current.TryFindResource("SuccesfullyInstalled"), (string)Application.Current.TryFindResource("InstallingTile"), System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
					}
					catch (Exception ex)
					{
						System.Windows.MessageBox.Show((string)Application.Current.TryFindResource("InstallingFailed") + "\n" + (string)Application.Current.TryFindResource("ErrorText") + ex.Message, (string)Application.Current.TryFindResource("InstallingTile"), System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);
					}
				}

			}
		}

		static void installButton_Click(object sender, EventArgs e)
		{
			td.Close(TaskDialogResult.Ok);

			try
			{
				Slate.Packaging.PackageManager.Unpack(LongBarMain.sett.path, tilePath);

				if (longBar != null)
				{
					string name = Path.GetFileNameWithoutExtension(tilePath);
					LongBarMain.Tiles.Add(new Tile(LongBarMain.sett.path + "\\Library\\" + name + "\\" + name + ".dll"));
					MenuItem item = new MenuItem();
					item.Header = name;
					item.Click += new RoutedEventHandler(longBar.AddTileSubItem_Click);
					longBar.AddTileItem.Items.Add(item);
					LongBarMain.Tiles[LongBarMain.Tiles.Count - 1].Load(LongBarMain.sett.side, double.NaN);
					if (!LongBarMain.Tiles[LongBarMain.Tiles.Count-1].hasErrors)
					{
						longBar.TilesGrid.Children.Insert(0, LongBarMain.Tiles[LongBarMain.Tiles.Count-1]);
						((MenuItem)longBar.AddTileItem.Items[((MenuItem)longBar.AddTileItem).Items.Count - 1]).IsChecked = true;
					}
				}

				tdResult = new TaskDialog();
				tdResult.Icon = TaskDialogStandardIcon.Information;
				tdResult.Caption = (string)Application.Current.TryFindResource("InstallingTile");

				tdResult.InstructionText = (string)Application.Current.TryFindResource("TileInstalled");
				tdResult.Text = tileName + " " + (string)Application.Current.TryFindResource("SuccesfullyInstalled");

				tdResult.StandardButtons = TaskDialogStandardButtons.Ok;

				tdResult.Show();

			}
			catch (Exception ex)
			{
				tdResult = new TaskDialog();
				tdResult.Icon = TaskDialogStandardIcon.Error;
				tdResult.Caption = (string)Application.Current.TryFindResource("InstallingTile");

				tdResult.InstructionText = (string)Application.Current.TryFindResource("CantInstallTile");
				tdResult.Text = (string)Application.Current.TryFindResource("InstallingFailed") + "\n" + (string)Application.Current.TryFindResource("ErrorText") + " " + ex.Message;

				tdResult.DetailsExpandedLabel = (string)Application.Current.TryFindResource("HideDetails");
				tdResult.DetailsCollapsedLabel = (string)Application.Current.TryFindResource("ShowDetails");
				tdResult.DetailsExpandedText = ex.ToString();

				tdResult.ExpansionMode = TaskDialogExpandedDetailsLocation.ExpandFooter;

				tdResult.StandardButtons = TaskDialogStandardButtons.Ok;

				tdResult.Show();
			}
		}
	}
}
