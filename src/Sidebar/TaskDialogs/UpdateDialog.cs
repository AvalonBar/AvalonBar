using System;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Sidebar.TaskDialogs
{
    public class UpdateDialog
    {
        private static TaskDialog td;

        public static void ShowDialog(string version, string description)
        {
            td = new TaskDialog();
            td.Cancelable = true;
            td.Icon = TaskDialogStandardIcon.None;

            td.Caption = (string)Application.Current.TryFindResource("UpdateDialogTitle");
            td.Text = string.Format(
                (string)Application.Current.TryFindResource("UpdateDialogText"), version, VersionInfo.Core);
            td.InstructionText = (string)Application.Current.TryFindResource("UpdateDialogHeader");
            td.StandardButtons = TaskDialogStandardButtons.None;

            td.ExpansionMode = TaskDialogExpandedDetailsLocation.ExpandFooter;
            td.DetailsExpandedLabel = (string)Application.Current.TryFindResource("HideDetails");
            td.DetailsExpandedText = description;
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
            UpdateDownloadDialog.ShowDialog();
        }
    }
}
