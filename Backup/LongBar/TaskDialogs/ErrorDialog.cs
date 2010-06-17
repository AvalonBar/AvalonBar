using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using System.Reflection;
using Microsoft.WindowsAPICodePack;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using Microsoft.Win32;
using System.Runtime.InteropServices;
namespace LongBar.TaskDialogs
{
    public class ErrorDialog
    {
        private static SmtpClient client = new SmtpClient("smtp.live.com",25);
        //private static MailMessage msg = new MailMessage();
        private static Exception ex = null;

        public static void ShowDialog(string caption, string errorText, Exception exception)
        {
            ex = exception;
            // Error dialog
            if (Environment.OSVersion.Version.Major >= 6)
            {

                TaskDialog tdError = new TaskDialog();
                tdError.DetailsExpanded = false;
                tdError.Cancelable = true;
                tdError.Icon = TaskDialogStandardIcon.Error;

                tdError.Caption = (string)Application.Current.TryFindResource("LongBarError");
                tdError.InstructionText = caption;
                tdError.Text = errorText;
                tdError.DetailsExpandedLabel = (string)Application.Current.TryFindResource("Hide details");
                tdError.DetailsCollapsedLabel = (string)Application.Current.TryFindResource("Show details");
                tdError.DetailsExpandedText = ex.ToString();

                tdError.ExpansionMode = TaskDialogExpandedDetailsLocation.ExpandFooter;

                TaskDialogCommandLink sendButton = new TaskDialogCommandLink("sendButton", (string)Application.Current.TryFindResource("SendFeedback1"), (string)Application.Current.TryFindResource("SendFeedback2"));
                sendButton.Click += new EventHandler(sendButton_Click);

                TaskDialogCommandLink dontSendButton = new TaskDialogCommandLink("dontSendButton", (string)Application.Current.TryFindResource("DontSendFeedback1"), (string)Application.Current.TryFindResource("DontSendFeedback2"));
                dontSendButton.Click += new EventHandler(dontSendButton_Click);

                tdError.Controls.Add(sendButton);
                tdError.Controls.Add(dontSendButton);

                tdError.Show();
            }
            else
            {
                System.Windows.MessageBox.Show(errorText, (string)Application.Current.TryFindResource("ErrorOccured2"), System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        static void dontSendButton_Click(object sender, EventArgs e)
        {
            /*if (tdError != null)
                tdError.Close(TaskDialogResult.Cancel);*/
           ((TaskDialog)((TaskDialogControl)sender).HostingDialog).Close(TaskDialogResult.Close);
        }

        static TaskDialogProgressBar sendFeedbackProgressBar;
        static TaskDialog tdSendFeedback;
        static void sendButton_Click(object sender, EventArgs e)
        {
            ((TaskDialog)((TaskDialogControl)sender).HostingDialog).Close(TaskDialogResult.Close);
            // Send feedback button
            tdSendFeedback = new TaskDialog();
            tdSendFeedback.Cancelable = true;

            tdSendFeedback.Caption = (string)Application.Current.TryFindResource("SendFeedbackDialog");
            tdSendFeedback.Text = (string)Application.Current.TryFindResource("SendingFeedback");

            // Show a progressbar
            sendFeedbackProgressBar = new TaskDialogProgressBar(0, 100, 0);
            tdSendFeedback.ProgressBar = sendFeedbackProgressBar;

            /*if (tdError != null)
                tdError.Close(TaskDialogResult.Ok);*/
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress("longbarbugs@hotmail.com", "LongBar Bugs Service");
            msg.To.Clear();
            msg.To.Add("stealth2008@live.ru");
            msg.Subject = "LongBar Bug Report";

            Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;

            msg.Body = "LongBar version: " + string.Format("LongBar Slate {0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision) +
                "\nOS Version: " + Environment.OSVersion.ToString() +
                "\nException source: " + ex.Source +
                "\nException:\n" + ex.ToString();
            System.Net.NetworkCredential cr = new System.Net.NetworkCredential("longbarbugs@hotmail.com", "123123");
            client.SendCompleted += new SendCompletedEventHandler(client_SendCompleted);
            client.UseDefaultCredentials = false;
            client.Credentials = cr;
            client.EnableSsl = true;
            client.SendAsync(msg, null);

            tdSendFeedback.ProgressBar.State = TaskDialogProgressBarState.Marquee;
            tdSendFeedback.Show();
        }

        static void client_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            tdSendFeedback.ProgressBar.Value = 100;
            tdSendFeedback.ProgressBar.State = TaskDialogProgressBarState.Paused;
            if (e.Error == null)
            {
                tdSendFeedback.InstructionText = (string)Application.Current.TryFindResource("ThanksForFeedback");
                tdSendFeedback.Text = (string)Application.Current.TryFindResource("Idontthinkso");
            }
            else
            {
                tdSendFeedback.InstructionText = (string)Application.Current.TryFindResource("SendingFailed");
                tdSendFeedback.Text = (string)Application.Current.TryFindResource("SendingError") + e.Error.Message;
            }
        }
    }
}
