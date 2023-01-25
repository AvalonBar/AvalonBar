using System;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Net;
using System.IO;
using System.Reflection;
using System.Windows;
using System.ComponentModel;
using System.Diagnostics;

namespace Sidebar
{
    public class UpdateDownloadDialog
    {
        private static TaskDialog td;

        public static void ShowDialog(string packageUrl)
        {
            td = new TaskDialog();
            td.Caption = (string)Application.Current.TryFindResource("UpdateDownloadDialogTitle");
            td.InstructionText = (string)Application.Current.TryFindResource("UpdateDownloadDialogHeader");
            td.StandardButtons = TaskDialogStandardButtons.Cancel;

            TaskDialogProgressBar progressBar = new TaskDialogProgressBar("progressBar");
            progressBar.Maximum = 100;

            td.Controls.Add(progressBar);
            td.Closing += new EventHandler<TaskDialogClosingEventArgs>(tdDownload_Closing);

            if (!Directory.Exists(Settings.Current.path + "\\Updates"))
            {
                Directory.CreateDirectory(Settings.Current.path + "\\Updates");
            }

            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
            client.DownloadFileAsync(new Uri(packageUrl), Settings.Current.path + "\\Updates\\Update");

            td.Show();
        }

        private static WebClient client = new WebClient();

        static void tdDownload_Closing(object sender, TaskDialogClosingEventArgs e)
        {
            if (e.TaskDialogResult == TaskDialogResult.Cancel && client.IsBusy)
                client.CancelAsync();
        }

        static void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
            {
                Services.UpdateFiles(Settings.Current.path);
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    Application.Current.Shutdown();
                }, null);
                // TODO: Usage of hardcoded executable name
                Process.Start(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\LongBar.exe");
                td.Close();
            }
        }

        static void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ((TaskDialogProgressBar)td.Controls["progressBar"]).Value = e.ProgressPercentage;
        }
    }
}
