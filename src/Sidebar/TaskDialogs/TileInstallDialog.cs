using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAPICodePack;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Win32;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using Sidebar.Core;

namespace Sidebar.TaskDialogs
{
    public class TileInstallDialog
    {
        private static TaskDialog td = null;
        private static TaskDialog tdResult = null;

        private static string tilePath;
        private static string tileName;

        private static SidebarWindow longBar;

        public static void ShowDialog(SidebarWindow longbar, string name, string path)
        {
            longBar = longbar;

            tilePath = path;
            tileName = name;

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

        static void installButton_Click(object sender, EventArgs e)
        {
            td.Close(TaskDialogResult.Ok);

            try
            {
                PackageManager.Unpack(SidebarWindow.sett.path, tilePath);

                if (longBar != null)
                {
                    string name = Path.GetFileNameWithoutExtension(tilePath);
                    SidebarWindow.Tiles.Add(new Tile(SidebarWindow.sett.path + "\\Library\\" + name + "\\" + name + ".dll"));
                    MenuItem item = new MenuItem();
                    item.Header = name;
                    item.Click += new RoutedEventHandler(longBar.AddTileSubItem_Click);
                    longBar.AddTileItem.Items.Add(item);
                    SidebarWindow.Tiles[SidebarWindow.Tiles.Count - 1].Load(SidebarWindow.sett.side, double.NaN);
                    if (!SidebarWindow.Tiles[SidebarWindow.Tiles.Count - 1].hasErrors)
                    {
                        longBar.TilesGrid.Children.Insert(0, SidebarWindow.Tiles[SidebarWindow.Tiles.Count - 1]);
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
