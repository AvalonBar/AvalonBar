using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Windows.Media.Animation;

namespace LongBar
{
	/// <summary>
	/// Interaction logic for Library.xaml
	/// </summary>
	public partial class Library : Window
	{
		private LongBarMain longbar;
		private WebClient dowloader;
		private ToolButton DownloadButton;
		private ToolButton LocalTilesButton;
		private ToolButton OnlineTilesButton;

		private int _selectedIndex = -1;

		public int SelectedIndex
		{
			get { return _selectedIndex; }
			set
			{
				if (value == _selectedIndex)
					return;

				if (value > -1)
				{
					if (ViewMode == 0) {
						ItemsCount.Visibility = Visibility.Collapsed;

						WrapPanel1.Visibility = Visibility.Visible;
						WrapPanel2.Visibility = Visibility.Visible;

						CurrentItemTitle.Text = ((LibraryItem)DownTilesPanel.Children[value]).Header;
						CurrentItemDescription.Text = ((LibraryItem)DownTilesPanel.Children[value]).Description;
						CurrentItemAuthor.Text = ((LibraryItem)DownTilesPanel.Children[value]).Developer;
						CurrentItemVersion.Text = ((LibraryItem)DownTilesPanel.Children[value]).Version;
						CurrentItemAvailability.Text = ((LibraryItem)DownTilesPanel.Children[value]).TileAvailability;
						CurrentItemIcon.Source = ((LibraryItem)DownTilesPanel.Children[value]).ItemIconImage.Source;
						((LibraryItem)DownTilesPanel.Children[value]).Selected = true;
						if (DownloadButton.Visibility != Visibility.Visible)
						{
							DownloadButton.Visibility = Visibility.Visible;
							DownloadButton.Transfrom.BeginAnimation(TranslateTransform.YProperty, (DoubleAnimation)DownloadButton.Resources["ShowAnim1"]);
						}
					}
					if (ViewMode == 1) {
						ItemsCount.Visibility = Visibility.Collapsed;

						WrapPanel1.Visibility = Visibility.Visible;
						WrapPanel2.Visibility = Visibility.Visible;

						CurrentItemTitle.Text = ((LibraryItem)LongBarTilesPanel.Children[value]).Header;
						CurrentItemDescription.Text = ((LibraryItem)LongBarTilesPanel.Children[value]).Description;
						CurrentItemAuthor.Text = ((LibraryItem)LongBarTilesPanel.Children[value]).Developer;
						CurrentItemVersion.Text = ((LibraryItem)LongBarTilesPanel.Children[value]).Version;
						CurrentItemAvailability.Text = ((LibraryItem)LongBarTilesPanel.Children[value]).TileAvailability;
						CurrentItemIcon.Source = ((LibraryItem)LongBarTilesPanel.Children[value]).ItemIconImage.Source;
						((LibraryItem)LongBarTilesPanel.Children[value]).Selected = true;
					}
				}
				else
				{
					ItemsCount.Visibility = Visibility.Visible;
					WrapPanel1.Visibility = Visibility.Collapsed;
					WrapPanel2.Visibility = Visibility.Collapsed;

					//CurrentItemIconMS3.ImageSource = //new BitmapImage(new Uri("AvalonBar;component/Resources/Library_icon.png", UriKind.Absolute));
					DownloadButton.Visibility = Visibility.Collapsed;
				}

				if (_selectedIndex > -1)
					if (ViewMode == 0) {
						((LibraryItem)DownTilesPanel.Children[_selectedIndex]).Selected = false;
					}
					if (ViewMode == 1) {
						((LibraryItem)LongBarTilesPanel.Children[_selectedIndex]).Selected = false;
					}
				_selectedIndex = value;
			}
		}

		public Library(LongBarMain longbar)
		{
			InitializeComponent();

			this.longbar = longbar;
		}

		void DownloadButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (SelectedIndex > -1)
			{
				LoadingGrid.Visibility = Visibility.Visible;
				LoadingGrid.BeginAnimation(OpacityProperty, (DoubleAnimation)LoadingGrid.Resources["OpacityAnim1"]);
			}
		}

		private void DoubleAnimation_Completed(object sender, EventArgs e)
		{
			if (!Directory.Exists(LongBarMain.sett.Program.Path + @"\Cache"))
				Directory.CreateDirectory(LongBarMain.sett.Program.Path + @"\Cache");
			DownloadingStatusTextBlock.Text = (string)TryFindResource("NetConnecting");
			string url = ((LibraryItem)DownTilesPanel.Children[SelectedIndex]).Link;

			if (String.IsNullOrEmpty(url))
			{
				MessageBox.Show((string)TryFindResource("TileDlFailedConn"));
				LoadingGrid.Visibility = Visibility.Collapsed;
				LoadingGrid.Opacity = 0;
				return;
			}

			dowloader = new WebClient();
			dowloader.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(dowloader_DownloadFileCompleted);
			dowloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(dowloader_DownloadProgressChanged);
			DownloadingStatusTextBlock.Text = (string)TryFindResource("NetDownloading");

			dowloader.DownloadFileAsync(new Uri(url), LongBarMain.sett.Program.Path + @"\Cache\" + url.Substring(url.LastIndexOf("/") + 1));
		}

		void dowloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			ProgressBar.Value = e.ProgressPercentage;
			DownloadingProgressTextBlock.Text = string.Format((string)TryFindResource("TileDlProgress"), Math.Round((double)(e.BytesReceived / 1024)), Math.Round((double)(e.TotalBytesToReceive / 1024)));
		}

		void dowloader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			if (e.Error == null && !e.Cancelled) {
				TaskDialogs.TileInstallDialog.ShowDialog(longbar,
														 ((LibraryItem)DownTilesPanel.Children[SelectedIndex]).Header,
														 LongBarMain.sett.Program.Path + @"\Cache\" +
														 ((LibraryItem)DownTilesPanel.Children[SelectedIndex]).Header +
														 ".tile");
			} else if (!e.Cancelled) {
				MessageBox.Show((string)TryFindResource("TileDlFailed") + "\n" + (string)TryFindResource("TileDlError") + ": \n" + e.Error.ToString(), (string)TryFindResource("TileDlError"), MessageBoxButton.OK, MessageBoxImage.Error);
			}
			LoadingGrid.Visibility = Visibility.Collapsed;
			LoadingGrid.Opacity = 0;
		}

		private void CancelDownloadButton_Click(object sender, RoutedEventArgs e)
		{
			if (dowloader.IsBusy)
				dowloader.CancelAsync();
		}
		private void GetInstalledTiles()
		{
			foreach (Tile t in LongBarMain.Tiles)
			{
				LibraryItem item = new LibraryItem();
				if (t.Info != null)
				{
					item.Header = t.Info.Name;
					item.MouseLeftButtonDown += new MouseButtonEventHandler(item_MouseLeftButtonDown);
					item.MouseDoubleClick += new MouseButtonEventHandler(item_MouseDoubleClick);
					item.Developer = t.Info.Developer;
					item.Description = t.Info.Description;
					item.Version = t.Info.Version;
					item.TileAvailability = "Local Tile";
					item.Link = "http://www.example.com";
					//item.Icon = "";
					try { LongBarTilesPanel.Children.Add(item); } catch (Exception) {}
				}
			}
		}
		private void GetTiles()
		{
			string file = LongBarMain.sett.Program.Path + @"\Cache\Tiles.list";

			if (Directory.Exists(LongBarMain.sett.Program.Path + @"\Cache") && File.Exists(file))
			{
				FileInfo f = new FileInfo(LongBarMain.sett.Program.Path + @"\Cache\Tiles.list");
				if (Math.Abs(DateTime.Now.Day - f.LastAccessTime.Day) > 3)
				{
					f.Delete();
					GetTiles();
					return;
				}

				XmlTextReader reader = new XmlTextReader(file);
				LibraryItem item = new LibraryItem();
				while (reader.Read())
				{
					item.TileAvailability = "Online Tile";
					if (reader.NodeType == XmlNodeType.Element && reader.Name.ToLower().Equals("tile"))
					{
						item = new LibraryItem();
						item.MouseLeftButtonDown += new MouseButtonEventHandler(item_MouseLeftButtonDown);
					}
					if (reader.NodeType == XmlNodeType.EndElement && reader.Name.ToLower().Equals("tile"))
					{
						item.MouseDoubleClick += new MouseButtonEventHandler(item_MouseDoubleClick);
						// FIXME: Specified visual is already a child of another visual or the root of a composition target.
						// A workaround to prevent the error report from appearing.
						try { DownTilesPanel.Children.Add(item); } catch (Exception) {}
						//DownTilesCaption.Text = String.Format("Downloadable Tiles ({0})", DownTilesPanel.Children.Count);
						//ItemsCount.Text = String.Format("Elements: {0}", DownTilesPanel.Children.Count);
					}

					if (reader.NodeType == XmlNodeType.Element && reader.Name.ToLower().Equals("name"))
					{
						reader.MoveToContent();
						item.Header = reader.ReadElementString();
					}

					if (reader.NodeType == XmlNodeType.Element && reader.Name.ToLower().Equals("developer"))
					{
						reader.MoveToContent();
						item.Developer = reader.ReadElementString();
					}

					if (reader.NodeType == XmlNodeType.Element && reader.Name.ToLower().Equals("icon"))
					{
						reader.MoveToContent();
						item.Icon = reader.ReadElementString();
					}

					if (reader.NodeType == XmlNodeType.Element && reader.Name.ToLower().Equals("link"))
					{
						reader.MoveToContent();
						item.Link = reader.ReadElementString();
					}

					if (reader.NodeType == XmlNodeType.Element && reader.Name.ToLower().Equals("description"))
					{
						reader.MoveToContent();
						item.Description = reader.ReadElementString();
					}

					if (reader.NodeType == XmlNodeType.Element && reader.Name.ToLower().Equals("version"))
					{
						reader.MoveToContent();
						item.Version = reader.ReadElementString();
					}
				}
				reader.Close();
			}
			else
			{
				Directory.CreateDirectory(LongBarMain.sett.Program.Path + @"\Cache");

				WebClient client = new WebClient();
				client.DownloadFile(LongBarMain.sett.Links.TilesURL, LongBarMain.sett.Program.Path + @"\Cache\Tiles.list");

				GetTiles();				
			}
		}

		void item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			DownloadButton_MouseLeftButtonUp(sender, e);
		}

		void item_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (ViewMode == 0) {
				SelectedIndex = DownTilesPanel.Children.IndexOf((LibraryItem)sender);
			} else if (ViewMode == 1) {
				SelectedIndex = LongBarTilesPanel.Children.IndexOf((LibraryItem)sender);
			}
		}

		private void BottomBorderRect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Mouse.Capture(BottomBorderRect);
		}

		private void BottomBorderRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Mouse.Capture(null);
		}

		private void BottomBorderRect_MouseMove(object sender, MouseEventArgs e)
		{
			if (Mouse.Captured == BottomBorderRect &&
				((MainPanel.ActualHeight - e.GetPosition(MainPanel).Y) > 50
				&& (MainPanel.ActualHeight - e.GetPosition(MainPanel).Y) < 200))
			{
				DetailsPane.Height = MainPanel.ActualHeight - e.GetPosition(MainPanel).Y;
				DownTilesPanelScrollViewer.Height = BottomBorderRect.TransformToAncestor(MainPanel).Transform(new Point(0, 0)).Y - 64;
			}
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			double height = BottomBorderRect.TransformToAncestor(MainPanel).Transform(new Point(0, 0)).Y - 64;
			if (height > 0)
				DownTilesPanelScrollViewer.Height = height;
		}

		private void DownTilesPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.Source.GetType() != typeof(LibraryItem))
			{
				this.SelectedIndex = -1;

				foreach (LibraryItem item in SearchTilesPanel.Children)
				{
					if (item.Selected)
						item.Selected = false;
				}
			}
		}

		private void SearchField_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if ((bool)e.NewValue == false)
			{
				SearchField.Opacity = 0.7;
				if (String.IsNullOrEmpty(SearchField.Text))
				{
					SearchField.Text = "Search tile...";
					SearchField.FontStyle = FontStyles.Italic;
					SearchField.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF696969"));
				}
			}
			else
			{
				if (SearchField.Text == "Search tile...")
				{
					SearchField.Text = "";
					SearchField.FontStyle = FontStyles.Normal;
					SearchField.Foreground = Brushes.Black;
				}
				else
					SearchField.SelectAll();
			}
		}

		private void SearchField_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (SearchField.Text != "Search tile..." && !String.IsNullOrEmpty(SearchField.Text))
			{
				SearchTilesPanel.Children.Clear();
				SearchTiles.Visibility = Visibility.Visible;
				if (ViewMode == 0) {
					DownTiles.Visibility = Visibility.Collapsed;
					foreach (LibraryItem item in DownTilesPanel.Children)
					{
						if (item.Header.ToLower().Contains(SearchField.Text.ToLower()))
						{
							LibraryItem newItem = new LibraryItem();
							newItem.Header = item.Header;
							newItem.ItemIconImage.Source = item.ItemIconImage.Source;
							newItem.Developer = item.Developer;
							newItem.Description = item.Description;
							newItem.Link = item.Link;
							newItem.MouseLeftButtonDown += new MouseButtonEventHandler(newItem_MouseLeftButtonDown);
							SearchTilesPanel.Children.Add(newItem);
						}
					}

				}
				if (ViewMode == 1) {
					LongBarTiles.Visibility = Visibility.Collapsed;
					foreach (LibraryItem item in LongBarTilesPanel.Children)
					{
						if (item.Header.ToLower().Contains(SearchField.Text.ToLower()))
						{
							LibraryItem newItem = new LibraryItem();
							newItem.Header = item.Header;
							newItem.ItemIconImage.Source = item.ItemIconImage.Source;
							newItem.Developer = item.Developer;
							newItem.Description = item.Description;
							newItem.Link = item.Link;
							newItem.MouseLeftButtonDown += new MouseButtonEventHandler(newItem_MouseLeftButtonDown);
							SearchTilesPanel.Children.Add(newItem);
						}
					}
				}
				SearchTilesCaption.Text = "Found: " + SearchTilesPanel.Children.Count.ToString();
			}
			else if (DownTiles != null && DownTiles.Visibility == Visibility.Collapsed && ViewMode == 0)
			{
				SearchTiles.Visibility = Visibility.Collapsed;
				DownTiles.Visibility = Visibility.Visible;
				SearchTilesPanel.Children.Clear();
			}
			else if (LongBarTiles != null && LongBarTiles.Visibility == Visibility.Collapsed && ViewMode == 1)
			{
				SearchTiles.Visibility = Visibility.Collapsed;
				LongBarTiles.Visibility = Visibility.Visible;
				SearchTilesPanel.Children.Clear();
			}
		}

		void newItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			foreach (LibraryItem item in SearchTilesPanel.Children)
			{
				if (item.Selected)
					item.Selected = false;
			}
			((LibraryItem)sender).Selected = true;

			foreach (LibraryItem item in DownTilesPanel.Children)
			{
				if (item.Header == ((LibraryItem)sender).Header)
				{
					item_MouseLeftButtonDown(item, e);
					break;
				}
			}
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			DownloadButton = new ToolButton();
			DownloadButton.Visibility = Visibility.Collapsed;
			DownloadButton.Text = "Download tile";
			DownloadButton.MouseLeftButtonUp += new MouseButtonEventHandler(DownloadButton_MouseLeftButtonUp);

			LocalTilesButton = new ToolButton();
			LocalTilesButton.Visibility = Visibility.Visible;
			LocalTilesButton.Text = "Local Tiles";
			LocalTilesButton.MouseLeftButtonDown += LocalTilesButton_MouseLeftButtonDown;

			OnlineTilesButton = new ToolButton();
			OnlineTilesButton.Visibility = Visibility.Collapsed;
			OnlineTilesButton.Text = "Online Tiles";
			OnlineTilesButton.MouseLeftButtonDown += OnlineTilesButton_MouseLeftButtonDown;

			ToolBar.Children.Add(DownloadButton);
			ToolBar.Children.Add(LocalTilesButton);
			ToolBar.Children.Add(OnlineTilesButton);

			DownTilesPanelScrollViewer.Height = BottomBorderRect.TransformToAncestor(MainPanel).Transform(new Point(0, 0)).Y - 64;
			DownTilesCaption.Text = "Retrieving list of available tiles...";
		}
		// Viewmode 0 - OnlineTiles
		// Viewmode 1 - LocalTiles
		internal int ViewMode = 0;
		void LocalTilesButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (DownTiles.Visibility == Visibility.Visible) {
				DownTiles.Visibility = Visibility.Collapsed;
				LongBarTiles.Visibility = Visibility.Visible;
				LocalTilesButton.Visibility = Visibility.Collapsed;
				OnlineTilesButton.Visibility = Visibility.Visible;
				ViewMode = 1;
				RecalculateElementValues(1);
			}
		}

		void OnlineTilesButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (LongBarTiles.Visibility == Visibility.Visible) {
				LongBarTiles.Visibility = Visibility.Collapsed;
				DownTiles.Visibility = Visibility.Visible;
				OnlineTilesButton.Visibility = Visibility.Collapsed;
				LocalTilesButton.Visibility = Visibility.Visible;
				ViewMode = 0;
				RecalculateElementValues(2);
			}
		}
		private void DoubleAnimation_Completed_1(object sender, EventArgs e)
		{
			// Populates the Local Tiles panel
			GetInstalledTiles();
			// Connects to the internet and populates the Online Tiles panel
			GetTiles();
			// Recalculates the values
			// (since the first panel shown is online, calculate online tiles panel count)
			RecalculateElementValues(2);
		}

		private void RecalculateElementValues(int type)
		{
			// Calculate all TileLibrary items in the Panel.
			LongBarTilesCaption.Text = String.Format("Local Tiles ({0})", LongBarTilesPanel.Children.Count);
			DownTilesCaption.Text = String.Format("Online Tiles ({0})", DownTilesPanel.Children.Count);
			int TotalTiles = 0;
			if (type == 0) {
				TotalTiles = DownTilesPanel.Children.Count + LongBarTilesPanel.Children.Count;
			} else if (type == 1) {
				TotalTiles = LongBarTilesPanel.Children.Count;
			} else if (type == 2) {
				TotalTiles = DownTilesPanel.Children.Count;
			} else { TotalTiles = 0; }
			ItemsCount.Text = String.Format("Elements: {0}", TotalTiles);
		}
	}
}