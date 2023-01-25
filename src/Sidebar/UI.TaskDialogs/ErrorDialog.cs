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

namespace Sidebar
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

            tdError.Caption = Utils.FindString("LongBarError");
            tdError.InstructionText = caption;
            tdError.Text = errorText;
            tdError.DetailsExpandedLabel = Utils.FindString("HideDetails");
            tdError.DetailsCollapsedLabel = Utils.FindString("ShowDetails");
            tdError.DetailsExpandedText = ex.ToString();

            tdError.ExpansionMode = TaskDialogExpandedDetailsLocation.ExpandFooter;

            TaskDialogCommandLink sendButton = new TaskDialogCommandLink("sendButton",
                Utils.FindString("SendFeedback1"),
                Utils.FindString("SendFeedback2"));
            sendButton.Click += new EventHandler(sendButton_Click);

            TaskDialogCommandLink dontSendButton = new TaskDialogCommandLink("dontSendButton",
                Utils.FindString("DontSendFeedback1"),
                Utils.FindString("DontSendFeedback2"));
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
            Process.Start(Services.IssuesUrl);

            ((TaskDialog)((TaskDialogControl)sender).HostingDialog).Close(TaskDialogResult.Close);
        }
    }
}
