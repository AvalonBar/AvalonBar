using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Net;
using System.IO;
using System.Reflection;
using Sidebar.Core;

namespace Sidebar.TaskDialogs
{
    public class UpdateDialog
    {
        private static TaskDialog td;
        private static TaskDialog tdDownload;

        public static void ShowDialog(string build, string description)
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                td = new TaskDialog();
                td.Cancelable = true;
                td.Icon = TaskDialogStandardIcon.None;

                td.Caption = "LongBar 2.1 update";
                td.Text = "To see what's new in this version, click Show more info";
                td.InstructionText = string.Format("There is build {0} available. Do you want to update?", build);
                td.StandardButtons = TaskDialogStandardButtons.Cancel;

                td.ExpansionMode = TaskDialogExpandedDetailsLocation.ExpandFooter;
                td.DetailsExpandedLabel = "Show more info";
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
                System.Windows.MessageBox.Show(string.Format("There is build {0} available. Go to the https://sourceforge.net/projects/longbar to get the lates version.", build));
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
            tdDownload.Caption = "Downloading update";
            tdDownload.InstructionText = "Please wait while update is downloading.";
            tdDownload.Text = "LongBar will restart automatically.";
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
                UpdatesManager.UpdateFiles(LongBarMain.sett.path);
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    App.Current.Shutdown();
                }, null);
                LongBarMain.ShellExecute(IntPtr.Zero, "open", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\LongBar.exe", String.Empty, String.Empty, 1);
                tdDownload.Close();
            }
        }

        static void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ((TaskDialogProgressBar)tdDownload.Controls["progressBar"]).Value = e.ProgressPercentage;
        }
    }
}
