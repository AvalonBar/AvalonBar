using System;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Sidebar
{
    public class UpdateDialog
    {
        private static TaskDialog td;
        private static UpdateInfo info;

        public static void ShowDialog(UpdateInfo updateInfo)
        {
            info = updateInfo;
            td = new TaskDialog();
            td.Cancelable = true;
            td.Icon = TaskDialogStandardIcon.None;

            td.Caption = (string)Application.Current.TryFindResource("UpdateDialogTitle");
            td.Text = string.Format(
                (string)Application.Current.TryFindResource("UpdateDialogText"), info.Version, VersionInfo.Core);
            td.InstructionText = (string)Application.Current.TryFindResource("UpdateDialogHeader");
            td.StandardButtons = TaskDialogStandardButtons.None;

            td.ExpansionMode = TaskDialogExpandedDetailsLocation.ExpandFooter;
            td.DetailsExpandedLabel = (string)Application.Current.TryFindResource("HideDetails");
            td.DetailsExpandedText = info.Description;
            td.DetailsCollapsedLabel = (string)Application.Current.TryFindResource("ShowDetails");

            TaskDialogCommandLink updateButton = new TaskDialogCommandLink("updateButton", "Update");
            updateButton.Click += new EventHandler(updateButton_Click);

            TaskDialogCommandLink cancelButton = new TaskDialogCommandLink("cancelButton", "Cancel");
            cancelButton.Click += new EventHandler(cancelButton_Click);

            td.Controls.Add(updateButton);
            td.Controls.Add(cancelButton);

            td.Show();
        }

        private static void cancelButton_Click(object sender, EventArgs e)
        {
            td.Close(TaskDialogResult.Cancel);
        }

        private static void updateButton_Click(object sender, EventArgs e)
        {
            td.Close();
            UpdateDownloadDialog.ShowDialog(info.PackageUrl);
        }
    }
}
