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
using System.Windows.Controls;
namespace Sidebar.TaskDialogs
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

        static Window tdContacts;
        static TextBox mailBox;
        static TextBox commentBox;

        static void sendButton_Click(object sender, EventArgs e)
        {
            ((TaskDialog)((TaskDialogControl)sender).HostingDialog).Close(TaskDialogResult.Close);

            tdContacts = new Window();
            tdContacts.WindowStyle = WindowStyle.SingleBorderWindow;
            tdContacts.ResizeMode = ResizeMode.NoResize;
            tdContacts.Closed += new EventHandler(tdContacts_Closed);

            StackPanel p = new StackPanel();
            p.Margin = new Thickness(10);

            TextBlock text1 = new TextBlock();
            text1.Text = "Please type your e-mail, that developers can contact you:";
            text1.TextWrapping = TextWrapping.Wrap;

            mailBox = new TextBox();
            mailBox.Margin = new Thickness(0, 5, 0, 5);

            TextBlock text2 = new TextBlock();
            text2.Text = "Type a comment about error (what you did before it occured):";
            text2.TextWrapping = TextWrapping.Wrap;

            commentBox = new TextBox();
            commentBox.Height = 100;
            commentBox.Margin = new Thickness(0, 5, 0, 5);

            Button btn = new Button();
            btn.Content = "Send";
            btn.Width = 75;
            btn.Margin = new Thickness(0, 0, 10, 0);
            btn.HorizontalAlignment = HorizontalAlignment.Right;
            btn.Click += new RoutedEventHandler(btn_Click);

            p.Children.Add(text1);
            p.Children.Add(mailBox);
            p.Children.Add(text2);
            p.Children.Add(commentBox);
            p.Children.Add(btn);

            tdContacts.Content = p;
            tdContacts.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            tdContacts.Width = 400;
            tdContacts.Height = 250;
            tdContacts.ShowDialog();
        }

        static void tdContacts_Closed(object sender, EventArgs e)
        {
            SendFeedback();
        }

        static void btn_Click(object sender, RoutedEventArgs e)
        {
            tdContacts.Close();
        }

        static void SendFeedback()
        {

            // Send feedback dialog
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
            msg.Subject = "LongBar 2.1 Feedback";

            Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;

            msg.Body = "From: " + mailBox.Text + "\nComment: " + commentBox.Text + 
                "\n-------------------------------------------------------------------" + 
                "\nLongBar version: " + string.Format("LongBar Slate {0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision) +
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
