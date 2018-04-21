using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace LongBar
{
	/// <summary>
	/// Interaction logic for Notify.xaml
	/// </summary>
	public partial class Notify : Window
	{
		internal double staticLeft;
		internal double staticTop;

		/// <summary>
		/// Creates a new standard notification window without buttons or specifics.
		/// </summary>
		public Notify()
		{
			InitializeComponent();
			staticLeft = SystemInformation.WorkingArea.Right - Width -2;
	        staticTop = SystemInformation.WorkingArea.Bottom - Height -2;
	        Left = staticLeft;
	        Top = staticTop;
	        if (DismissalInteraction == true) {
				this.MouseLeftButtonDown += Notification_MouseLeftButtonDown_DismissInteract;
				DismissInteract.Opacity = 0;
			} else {
				DismissInteract.Visibility = Visibility.Hidden;
			}
		}
#region Change Parameters in notif.
		/// <summary>
		/// Change the header text in the notification window.
		/// </summary>
		/// <param name="text">The text in the header.</param>
		public void ChangeNotifyHeader(string text)
		{
			notifyHeader.Text = text;
		}

		/// <summary>
		/// Change the main content of the notification window.
		/// </summary>
		/// <remarks>To use new lines, simply use the /n tag.</remarks>
		/// <param name="text">The text inside the content box in the notification window.</param>
		public void ChangeContent(string text)
		{
			notifyContent.Text = text;
		}

		/// <summary>
		/// Change the icon beside the main content of the notification window.
		/// </summary>
		/// <param name="ig">Location of the image, must be compatible with ImageSource.</param>
		public void ChangeNotifyIcon(string ig)
		{
			notifyIcon.Source = (ImageSource)new ImageSourceConverter().ConvertFromString(ig);
		}
#endregion
		/// <summary>
		/// Shows the notification window with animations.
		/// </summary>
		public new void Show()
		{
			Opacity = 0;
			DoubleAnimation loadAnim = (DoubleAnimation)FindResource("LoadAnim");
			loadAnim.From = 0;
			loadAnim.To = 100;
			base.Show();
			BeginAnimation(OpacityProperty, loadAnim);
			MoveFromLow();
		}

		void Notification_MouseLeftButtonDown_DismissInteract(object sender, MouseButtonEventArgs e)
		{
			Environment.Exit(0);
		}
		/// <summary>
		/// Determines if the show dismiss button on highlight of the notification window
		/// is allowed to be executed.
		/// </summary>
		public void SetDismissalInteract(bool tf)
		{
			DismissalInteraction = tf;
		}

		bool DismissalInteraction;
		void Window_SourceInitialized(object sender, EventArgs e)
		{
			IntPtr handle = new WindowInteropHelper(this).Handle;
			if (Slate.DWM.DwmManager.IsGlassAvailable())
				Slate.DWM.DwmManager.EnableGlass(ref handle, IntPtr.Zero);
		}
		void MoveFromLow()
		{
			DoubleAnimation loadAnim = (DoubleAnimation)FindResource("TopAnim");
			loadAnim.From = Top + 10;
			loadAnim.To = Top;
			this.BeginAnimation(TopProperty, loadAnim);
		}
		void HoverAnim()
		{
			System.Windows.Input.Cursor csr = new System.Windows.Input.Cursor("C:/Windows/Cursors/aero_link.cur");
			Mouse.SetCursor(csr);
			if (Top == staticTop) {
				DoubleAnimation loadAnim = (DoubleAnimation)FindResource("TopAnim");
				loadAnim.From = Top;
				loadAnim.To = Top - 3;
				this.BeginAnimation(TopProperty, loadAnim);
			}
		}
		void HoverLeaveAnim()
		{
			if (Top == staticTop - 3) {
	        	DoubleAnimation loadAnim = (DoubleAnimation)FindResource("TopAnim");
	            loadAnim.From = Top;
	            loadAnim.To = Top + 3;
	            this.BeginAnimation(TopProperty, loadAnim);
			}
		}
		  private Storyboard myStoryboard;
		void Window_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (DismissalInteraction == true) {
				DoubleAnimation myDoubleAnimation = new DoubleAnimation();
				myDoubleAnimation.From = 0;
				myDoubleAnimation.To = 1.0;
				myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
				myStoryboard = new Storyboard();
				myStoryboard.Children.Add(myDoubleAnimation);
				Storyboard.SetTargetName(myDoubleAnimation, DismissInteract.Name);
				Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(Rectangle.OpacityProperty));
				myStoryboard.Begin(this);
			}
			HoverAnim();
		}
		void Window_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			HoverLeaveAnim();
		}
	}
}
