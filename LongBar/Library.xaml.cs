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
                    ItemsCount.Visibility = Visibility.Collapsed;

                    WrapPanel1.Visibility = Visibility.Visible;
                    WrapPanel2.Visibility = Visibility.Visible;

                    CurrentItemTitle.Text = ((LibraryItem)DownTilesPanel.Children[value]).Header;
                    CurrentItemDescription.Text = ((LibraryItem)DownTilesPanel.Children[value]).Description;
                    CurrentItemAuthor.Text = ((LibraryItem)DownTilesPanel.Children[value]).Developer;
                    CurrentItemVersion.Text = ((LibraryItem)DownTilesPanel.Children[value]).Version;
                    CurrentItemIcon.Source = ((LibraryItem)DownTilesPanel.Children[value]).ItemIconImage.Source;
                    ((LibraryItem)DownTilesPanel.Children[value]).Selected = true;
                    if (DownloadButton.Visibility != Visibility.Visible)
                    {
                        DownloadButton.Visibility = Visibility.Visible;
                        DownloadButton.Transfrom.BeginAnimation(TranslateTransform.YProperty, (DoubleAnimation)DownloadButton.Resources["ShowAnim1"]);
                    }
                }
                else
                {
                    ItemsCount.Visibility = Visibility.Visible;
                    WrapPanel1.Visibility = Visibility.Collapsed;
                    WrapPanel2.Visibility = Visibility.Collapsed;

                    CurrentItemIcon.Source = new BitmapImage(new Uri("/LongBar;component/Resources/Library_icon.png", UriKind.Relative));
                    DownloadButton.Visibility = Visibility.Collapsed;
                }

                if (_selectedIndex > -1)
                    ((LibraryItem)DownTilesPanel.Children[_selectedIndex]).Selected = false;
                _selectedIndex = value;
            }
        }

        public Library(LongBarMain longbar)
        {
            InitializeComponent();

            this.longbar = longbar;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            /*DownloadButton = new ToolButton();
            DownloadButton.Visibility = Visibility.Collapsed;
            DownloadButton.Text = "Download tile";
            DownloadButton.MouseLeftButtonUp += new MouseButtonEventHandler(DownloadButton_MouseLeftButtonUp);
            ToolBar.Children.Add(DownloadButton);

            DownTilesPanelScrollViewer.Height = BottomBorderRect.TransformToAncestor(MainPanel).Transform(new Point(0, 0)).Y - 64;
            GetTiles();*/
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
            if (!Directory.Exists(LongBarMain.sett.path + @"\Cache"))
                Directory.CreateDirectory(LongBarMain.sett.path + @"\Cache");
            DownloadingStatusTextBlock.Text = "Connecting...";
            string url = ((LibraryItem)DownTilesPanel.Children[SelectedIndex]).GeTileLink(); ;

            if (String.IsNullOrEmpty(url))
            {
                MessageBox.Show("Downloading tile failed. Can't connect to the server.");
                LoadingGrid.Visibility = Visibility.Collapsed;
                LoadingGrid.Opacity = 0;
                return;
            }

            dowloader = new WebClient();
            dowloader.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(dowloader_DownloadFileCompleted);
            dowloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(dowloader_DownloadProgressChanged);

            DownloadingStatusTextBlock.Text = "Downloading...";

            dowloader.DownloadFileAsync(new Uri(url), LongBarMain.sett.path + @"\Cache\" + url.Substring(url.LastIndexOf("/") + 1));


            /*if (!Directory.Exists(LongBarMain.sett.path + @"\Cache") || !File.Exists(LongBarMain.sett.path + @"\Cache\" + ((LibraryItem)DownTilesPanel.Children[SelectedIndex]).Header + ".tile"))
            {
                string url = ((LibraryItem)DownTilesPanel.Children[SelectedIndex]).GeTileLink();

                dowloader = new WebClient();
                dowloader.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(dowloader_DownloadFileCompleted);
                dowloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(dowloader_DownloadProgressChanged);

                dowloader.DownloadFileAsync(new Uri(url), LongBarMain.sett.path + @"\Cache\" + url.Substring(url.LastIndexOf("/") + 1));
            }
            else
            {
                FileInfo f = new FileInfo(LongBarMain.sett.path + @"\Cache\" + ((LibraryItem)DownTilesPanel.Children[SelectedIndex]).Header + ".tile");
                if (Math.Abs(DateTime.Now.Day - f.CreationTime.Day) > 3)
                {
                    f.Delete();
                    DoubleAnimation_Completed(sender, e);
                }
                else
                {
                    TaskDialogs.TileInstallDialog.ShowDialog(longbar, ((LibraryItem)DownTilesPanel.Children[SelectedIndex]).Header, LongBarMain.sett.path + @"\Cache\" + ((LibraryItem)DownTilesPanel.Children[SelectedIndex]).Header + ".tile");
                    LoadingGrid.Visibility = Visibility.Collapsed;
                    LoadingGrid.Opacity = 0;
                }

            }*/
        }

        void dowloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressBar.Value = e.ProgressPercentage;
            DownloadingProgressTextBlock.Text = string.Format("{0} Kb from {1} Kb", Math.Round((double)(e.BytesReceived / 1024)), Math.Round((double)(e.TotalBytesToReceive / 1024)));

        }

        void dowloader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
                TaskDialogs.TileInstallDialog.ShowDialog(longbar, ((LibraryItem)DownTilesPanel.Children[SelectedIndex]).Header, LongBarMain.sett.path + @"\Cache\" + ((LibraryItem)DownTilesPanel.Children[SelectedIndex]).Header + ".tile");

            LoadingGrid.Visibility = Visibility.Collapsed;
            LoadingGrid.Opacity = 0;
        }

        private void CancelDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (dowloader.IsBusy)
                dowloader.CancelAsync();
        }

        private void GetTiles()
        {
            string file = LongBarMain.sett.path + @"\Cache\Tiles.list";

            if (Directory.Exists(LongBarMain.sett.path + @"\Cache") && File.Exists(file))
            {
                FileInfo f = new FileInfo(LongBarMain.sett.path + @"\Cache\Tiles.list");
                if (Math.Abs(DateTime.Now.Day - f.CreationTime.Day) > 3)
                {
                    f.Delete();
                    GetTiles();
                    return;
                }

                XmlTextReader reader = new XmlTextReader(file);
                LibraryItem item = new LibraryItem();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name.ToLower().Equals("tile"))
                    {
                        item = new LibraryItem();
                        item.MouseLeftButtonDown += new MouseButtonEventHandler(item_MouseLeftButtonDown);
                    }
                    if (reader.NodeType == XmlNodeType.EndElement && reader.Name.ToLower().Equals("tile"))
                    {
                        item.MouseDoubleClick += new MouseButtonEventHandler(item_MouseDoubleClick);
                        DownTilesPanel.Children.Add(item);
                        DownTilesCaption.Text = String.Format("Downloadable Tiles ({0})", DownTilesPanel.Children.Count);
                        ItemsCount.Text = String.Format("Elements: {0}", DownTilesPanel.Children.Count);
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
                Directory.CreateDirectory(LongBarMain.sett.path + @"\Cache");

                WebRequest request = WebRequest.Create("http://cid-820d4d5cef8566bf.skydrive.live.com/self.aspx/LongBar%20Project/Library%202.0/Tiles.list");
                WebResponse response = request.GetResponse();

                StreamReader reader = new StreamReader(response.GetResponseStream());
                string line = "";

                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();

                    if (line.Contains(@"Tiles.list\x3fdownload\x26psid\x3d1', downloadUrl:"))
                    {
                        reader.Close();
                        response.Close();

                        line = line.Substring(line.IndexOf(@"Tiles.list\x3fdownload\x26psid\x3d1', downloadUrl:") + (@"Tiles.list\x3fdownload\x26psid\x3d1', downloadUrl:").Length + 2, line.IndexOf(@"Tiles.list\x3fdownload\x26psid\x3d1', demoteUrl:") - line.IndexOf(@"Tiles.list\x3fdownload\x26psid\x3d1', downloadUrl:") - 17);
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
                        line = line.Substring(0, line.Length - 9);
                        WebClient client = new WebClient();
                        client.DownloadFile(line, LongBarMain.sett.path + @"\Cache\Tiles.list");
                        break;
                    }
                }
                reader.Close();
                response.Close();

                GetTiles();
            }
        }

        void item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DownloadButton_MouseLeftButtonUp(sender, e);
        }

        //DownloadTileWindow window;

        /*void item_GetTileLinkFinished(string link)
        {
            window = new DownloadTileWindow(longbar, link);
            window.Show();
        }*/

        void item_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.SelectedIndex = DownTilesPanel.Children.IndexOf((LibraryItem)sender);
        }

        private static string StripTags(string line)
        {
            string result = Regex.Replace(line, @"<[^>]*>", "");
            return System.Web.HttpUtility.HtmlDecode(result);
        }

        private static string GetLink(string line)
        {
            Match m = Regex.Match(line, "href=\".*\"><");
            if (m != null)
            {
                string result = m.Value;
                return result.Substring(6, result.LastIndexOf('"') - 6);
            }
            return "";
        }

        private static BitmapImage GetIcon(string tileName)
        {
            if (!Directory.Exists(LongBarMain.sett.path + @"\Cache") || !File.Exists(LongBarMain.sett.path + @"\Cache\" + tileName + ".png"))
            {
                Directory.CreateDirectory(LongBarMain.sett.path + @"\Cache");

                try
                {
                    WebRequest request = WebRequest.Create(String.Format("http://cid-820d4d5cef8566bf.skydrive.live.com/self.aspx/LongBar%20Project/Library%202.0/Icons/{0}.png", tileName));
                    WebResponse response = request.GetResponse();

                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string line = "";
                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLine();

                        if (line.Contains(tileName + ".png\x3fpsid\x3d1', downloadUrl:"))
                        {
                            reader.Close();
                            response.Close();

                            line = line.Substring(line.IndexOf(tileName + ".png\x3fpsid\x3d1', downloadUrl:") + (tileName + ".png\x3fpsid\x3d1', downloadUrl:").Length + 2, line.IndexOf(tileName + @".png\x3fdownload\x3fpsid\x3d1'") - line.IndexOf(tileName + ".png\x3fpsid\x3d1', downloadUrl:") - 5); 
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
                            System.Net.WebClient client = new WebClient();
                            client.DownloadFile(line, LongBarMain.sett.path + @"\Cache\" + tileName + ".png");
                            return new BitmapImage(new Uri(LongBarMain.sett.path + @"\Cache\" + tileName + ".png"));
                        }
                    }
                    reader.Close();
                    response.Close();
                    return new BitmapImage(new Uri("/LongBar;component/Resources/Tile_Icon.png", UriKind.Relative));
                }
                catch
                {
                    return new BitmapImage(new Uri("/LongBar;component/Resources/Tile_Icon.png", UriKind.Relative));
                }
            }
            else
            {
                DirectoryInfo d = new DirectoryInfo(LongBarMain.sett.path + @"\Cache");
                if (Math.Abs(DateTime.Now.Day - d.CreationTime.Day) > 7)
                    d.Delete(true);

                BitmapImage image = new BitmapImage(new Uri(LongBarMain.sett.path + @"\Cache\" + tileName + ".png"));
                return image;
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
                BottomGrid.Height = MainPanel.ActualHeight - e.GetPosition(MainPanel).Y;
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
                //foreach (LibraryItem item in SearchTilesPanel.Children)
                //{
                //    if (!item.Header.Contains(SearchField.Text))
                //        SearchTilesPanel.Children.Remove(item);
                //}
                SearchTilesPanel.Children.Clear();
                SearchTiles.Visibility = Visibility.Visible;
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
                SearchTilesCaption.Text = "Found: " + SearchTilesPanel.Children.Count.ToString();
            }
            else if (DownTiles != null && DownTiles.Visibility == Visibility.Collapsed)
            {
                SearchTiles.Visibility = Visibility.Collapsed;
                DownTiles.Visibility = Visibility.Visible;
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
            ToolBar.Children.Add(DownloadButton);

            DownTilesPanelScrollViewer.Height = BottomBorderRect.TransformToAncestor(MainPanel).Transform(new Point(0, 0)).Y - 64;
            DownTilesCaption.Text = "Getting list of available tiles...";
        }

        private void DoubleAnimation_Completed_1(object sender, EventArgs e)
        {
            GetTiles();
        }

    }
}
