using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Net;
using System.IO;
using System.Windows;
using System.Runtime.InteropServices;
using System.Reflection;

namespace LongBar.TaskDialogs
{
    public class UpdateDialog
    {
    	//TODO: Update the Update System
        private static TaskDialog td;
        private static TaskDialog tdDownload;

        public static void ShowDialog(string build, string description)
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                td = new TaskDialog();
                td.Cancelable = true;
                td.Icon = TaskDialogStandardIcon.None;

                td.Caption = string.Format((string)Application.Current.TryFindResource("UpdateDlgAlertCaption"), Application.Current.TryFindResource("ApplicationName"));
                td.Text = (string)Application.Current.TryFindResource("UpdateDlgWhatsNew");
                td.InstructionText = string.Format("There is build {0} available. Do you want to update?", build);
                td.StandardButtons = TaskDialogStandardButtons.Cancel;

                td.ExpansionMode = TaskDialogExpandedDetailsLocation.ExpandFooter;
                td.DetailsExpandedLabel = (string)Application.Current.TryFindResource("ShowDetails");
                td.DetailsCollapsedLabel = (string)Application.Current.TryFindResource("HideDetails");
                td.DetailsExpandedText = description;

                TaskDialogCommandLink updateButton = new TaskDialogCommandLink("updateButton", "Update", "After updating LongBar will be restarted.");
                updateButton.Click += new EventHandler(updateButton_Click);

                TaskDialogCommandLink cancelButton = new TaskDialogCommandLink("cancelButton", "No, thanks", "Continue work with current version.");
                cancelButton.Click += new EventHandler(cancelButton_Click);

                td.Controls.Add(updateButton);
                td.Controls.Add(cancelButton);

                td.Show();
            }
            else
            {
            	MessageBox.Show(string.Format((string)Application.Current.TryFindResource("LegacyUpdateCaption"), GitInfo.Repository, build, Slate.Data.XMLReader.ReadSettings("Links", "ProjectURL")));
            }
        }

        static void cancelButton_Click(object sender, EventArgs e)
        {
            td.Close(TaskDialogResult.Close);
        }

        private static WebClient client = new WebClient();

        static void updateButton_Click(object sender, EventArgs e)
        {
            td.Close();

            tdDownload = new TaskDialog();
            tdDownload.Caption = (string)Application.Current.TryFindResource("UpdateDlgCaption");
            tdDownload.InstructionText = (string)Application.Current.TryFindResource("UpdateDlgInstruct");
            tdDownload.Text = (string)Application.Current.TryFindResource("UpdateDlgText");
            tdDownload.StandardButtons = TaskDialogStandardButtons.Cancel;

            TaskDialogProgressBar progressBar = new TaskDialogProgressBar("progressBar");
            progressBar.Maximum = 100;

            tdDownload.Controls.Add(progressBar);
            tdDownload.Closing += new EventHandler<TaskDialogClosingEventArgs>(tdDownload_Closing);

            if (!Directory.Exists(LongBarMain.sett.path + "\\Updates"))
                Directory.CreateDirectory(LongBarMain.sett.path + "\\Updates");

            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(client_DownloadFileCompleted);

            client.DownloadFileAsync(new Uri("https://sourceforge.net/projects/longbar/files/Debug/LongBar%202.1/Updates/Update.data/download"), LongBarMain.sett.path + "\\Updates\\Update");

            tdDownload.Show();
        }

        static void tdDownload_Closing(object sender, TaskDialogClosingEventArgs e)
        {
            if (e.TaskDialogResult == TaskDialogResult.Cancel && client.IsBusy)
                client.CancelAsync();
        }

        static void client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
            {
                Slate.Updates.UpdatesManager.UpdateFiles(LongBarMain.sett.path);
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    App.Current.Shutdown();
                }, null);
                Process.Start(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\AvalonBar.exe");
                tdDownload.Close();
            }
        }

        static void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ((TaskDialogProgressBar)tdDownload.Controls["progressBar"]).Value = e.ProgressPercentage;
        }
    }
}
