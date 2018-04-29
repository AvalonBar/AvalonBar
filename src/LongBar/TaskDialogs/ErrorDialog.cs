using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using System.Reflection;
using Microsoft.WindowsAPICodePack;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace LongBar.TaskDialogs
{
	public class ErrorDialog
	{
		private static Exception ex = null;

		public static void ShowDialog(string caption, string errorText, Exception exception)
		{
			ex = exception;
			// Error dialog
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

				TaskDialogCommandLink sendButton = new TaskDialogCommandLink("sendButton", (string)Application.Current.TryFindResource("SendFeedback1"), (string)Application.Current.TryFindResource("SendFeedback2"));
				sendButton.Click += sendButton_Click;

				TaskDialogCommandLink dontSendButton = new TaskDialogCommandLink("dontSendButton", (string)Application.Current.TryFindResource("DontSendFeedback"));
				dontSendButton.Click += dontSendButton_Click;


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
			((TaskDialog)((TaskDialogControl)sender).HostingDialog).Close(TaskDialogResult.Close);

			Assembly assembly = Assembly.GetExecutingAssembly();
			Version version = assembly.GetName().Version;

			string msg =
				"\n-------------------------------------------------------------------" +
				"\nVersion: " + string.Format("{0} {1}.{2}.{3}.{4}", GitInfo.Milestone,
				                                      version.Major, version.Minor, version.Build,
				                                      version.Revision) +
				"\nBuilt from: " + string.Format("Repository - {0}, Branch - {1}, Milestone - {2}",
				                                          GitInfo.Repository, GitInfo.Branch,
				                                          GitInfo.Milestone) +
				"\nOS Version: " + Environment.OSVersion +
				"\nException source: " + ex.Source +
				"\nException:\n" + ex;

			Clipboard.SetText(msg);
			Process.Start(LongBarMain.sett.Links.BugTrackerURL);
		}
	}
}