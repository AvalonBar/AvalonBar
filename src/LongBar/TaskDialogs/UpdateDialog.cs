using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Net;
using System.Diagnostics;
using System.Windows;
using System.IO;
using System.Reflection;

namespace LongBar.TaskDialogs
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

                td.Caption = string.Format("LongBar {0} update", AssemblyInfo.SharedIV);
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
            	MessageBox.Show(string.Format((string)Application.Current.TryFindResource("LegacyUpdateCaption"), GitInfo.Repository, build, Slate.Data.XMLReader.ReadSettings("ProjectMisc", "ProjectLink")));
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
            tdDownload.Closing += (tdDownload_Closing);

            if (!Directory.Exists(LongBarMain.sett.path + "\\Updates"))
                Directory.CreateDirectory(LongBarMain.sett.path + "\\Updates");

            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(client_DownloadFileCompleted);

            client.DownloadFileAsync(new Uri(GetDirectLink()), LongBarMain.sett.path + "\\Updates\\Update");

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
                
                Process.Start(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\HornSide.exe");
                tdDownload.Close();
            }
        }

        static void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ((TaskDialogProgressBar)tdDownload.Controls["progressBar"]).Value = e.ProgressPercentage;
        }

        private static string GetDirectLink()
        {
            WebRequest request = WebRequest.Create("http://cid-820d4d5cef8566bf.skydrive.live.com/self.aspx/LongBar%20Project/Updates%202.0/Update.data");
            WebResponse response = request.GetResponse();

            StreamReader reader = new StreamReader(response.GetResponseStream());
            string line = "";
            string result = "";

            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();

                if (line.Contains(@"Update.data\x3fdownload\x26psid\x3d1', downloadUrl:"))
                {
                    reader.Close();
                    response.Close();

                    line = line.Substring(line.IndexOf(@"Update.data\x3fdownload\x26psid\x3d1', downloadUrl:") + (@"Update.data\x3fdownload\x26psid\x3d1', downloadUrl:").Length + 2, line.IndexOf(@"Update.data\x3fdownload\x26psid\x3d1', demoteUrl:") - line.IndexOf(@"Update.data\x3fdownload\x26psid\x3d1', downloadUrl:") - 17);
                    while (line.Contains(@"\x3a"))
                        line = line.Replace(@"\x3a", ":");
                    while (line.Contains(@"\x2f"))
                        line = line.Replace(@"\x2f", "/");
                    while (line.Contains(@"\x3f"))
                        line = line.Replace(@"\x3f", "?");
                    while (line.Contains(@"\x26"))
                        line = line.Replace(@"\x26", "&");
                    while (line.Contains(@"\x3d"))
                        line = line.Replace(@"\x3d", "=");
                    line = line.Substring(0, line.Length - 16);
                    result = line;
                    break;
                }
            }
            reader.Close();
            response.Close();

            return result;
        }
    }
}
