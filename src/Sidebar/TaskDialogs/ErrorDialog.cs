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
using System.Diagnostics;
using Sidebar.Core;

namespace Sidebar.TaskDialogs
{
    public class ErrorDialog
    {
        private static Exception ex = null;

        public static void ShowDialog(string caption, string errorText, Exception exception)
        {
            ex = exception;

            TaskDialog tdError = new TaskDialog();
            tdError.DetailsExpanded = false;
            tdError.Cancelable = true;
            tdError.Icon = TaskDialogStandardIcon.Error;

            tdError.Caption = (string)Application.Current.TryFindResource("LongBarError");
            tdError.InstructionText = caption;
            tdError.Text = errorText;
            tdError.DetailsExpandedLabel = (string)Application.Current.TryFindResource("HideDetails");
            tdError.DetailsCollapsedLabel = (string)Application.Current.TryFindResource("ShowDetails");
            tdError.DetailsExpandedText = ex.ToString();

            tdError.ExpansionMode = TaskDialogExpandedDetailsLocation.ExpandFooter;

            TaskDialogCommandLink sendButton = new TaskDialogCommandLink("sendButton",
                (string)Application.Current.TryFindResource("SendFeedback1"),
                (string)Application.Current.TryFindResource("SendFeedback2"));
            sendButton.Click += new EventHandler(sendButton_Click);

            TaskDialogCommandLink dontSendButton = new TaskDialogCommandLink("dontSendButton",
                (string)Application.Current.TryFindResource("DontSendFeedback1"),
                (string)Application.Current.TryFindResource("DontSendFeedback2"));
            dontSendButton.Click += new EventHandler(dontSendButton_Click);

            tdError.Controls.Add(sendButton);
            tdError.Controls.Add(dontSendButton);

            tdError.Show();
        }

        static void dontSendButton_Click(object sender, EventArgs e)
        {
            ((TaskDialog)((TaskDialogControl)sender).HostingDialog).Close(TaskDialogResult.Close);
        }

        static void sendButton_Click(object sender, EventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;

            string report =
                "\n-------------------------------------------------------------------" +
                "\nApplication version: " + version.ToString() +
                "\nOS Version: " + Environment.OSVersion.ToString() +
                "\nException source: " + ex.Source +
                "\nException:\n" + ex.ToString();

            Clipboard.SetData(DataFormats.Text, report);
            Process.Start(Services.Issues);

            ((TaskDialog)((TaskDialogControl)sender).HostingDialog).Close(TaskDialogResult.Close);
        }
    }
}
