using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAPICodePack;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Win32;
using System.Windows;
using System.IO;
using System.Windows.Controls;

namespace Sidebar
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

            td.Caption = Utils.FindString("InstallingTile");
            td.Text = path;
            td.InstructionText = string.Format(Utils.FindString("Dontdoit"), tileName);
            td.StandardButtons = TaskDialogStandardButtons.Cancel;

            TaskDialogCommandLink installButton = new TaskDialogCommandLink(
                "installButton",
                Utils.FindString("InstallThisIncredibleTileForMePlease1"));
            installButton.Click += new EventHandler(installButton_Click);

            td.Controls.Add(installButton);
            td.Show();
        }

        static void installButton_Click(object sender, EventArgs e)
        {
            td.Close(TaskDialogResult.Ok);

            try
            {
                AssetManager.Unpack(Settings.Current.path, tilePath);

                if (longBar != null)
                {
                    string name = Path.GetFileNameWithoutExtension(tilePath);
                    SidebarWindow.Tiles.Add(new Tile(Settings.Current.path + "\\Library\\" + name + "\\" + name + ".dll"));
                    MenuItem item = new MenuItem();
                    item.Header = name;
                    item.Click += new RoutedEventHandler(longBar.AddTileSubItem_Click);
                    longBar.AddTileItem.Items.Add(item);
                    SidebarWindow.Tiles[SidebarWindow.Tiles.Count - 1].Load(Settings.Current.side, double.NaN);
                    if (!SidebarWindow.Tiles[SidebarWindow.Tiles.Count - 1].hasErrors)
                    {
                        longBar.TilesGrid.Children.Insert(0, SidebarWindow.Tiles[SidebarWindow.Tiles.Count - 1]);
                        ((MenuItem)longBar.AddTileItem.Items[((MenuItem)longBar.AddTileItem).Items.Count - 1]).IsChecked = true;
                    }
                }

                tdResult = new TaskDialog();
                tdResult.Icon = TaskDialogStandardIcon.Information;
                tdResult.Caption = Utils.FindString("InstallingTile");

                tdResult.InstructionText = Utils.FindString("TileInstalled");
                tdResult.Text = tileName + " " + Utils.FindString("SuccesfullyInstalled");

                tdResult.StandardButtons = TaskDialogStandardButtons.Ok;

                tdResult.Show();

            }
            catch (Exception ex)
            {
                tdResult = new TaskDialog();
                tdResult.Icon = TaskDialogStandardIcon.Error;
                tdResult.Caption = Utils.FindString("InstallingTile");

                tdResult.InstructionText = Utils.FindString("CantInstallTile");
                tdResult.Text = Utils.FindString("InstallingFailed") + "\n" + Utils.FindString("ErrorText") + " " + ex.Message;

                tdResult.DetailsExpandedLabel = Utils.FindString("HideDetails");
                tdResult.DetailsCollapsedLabel = Utils.FindString("ShowDetails");
                tdResult.DetailsExpandedText = ex.ToString();

                tdResult.ExpansionMode = TaskDialogExpandedDetailsLocation.ExpandFooter;

                tdResult.StandardButtons = TaskDialogStandardButtons.Ok;

                tdResult.Show();
            }
        }
    }
}
