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
using System.Windows.Threading;
using System.Xml.Serialization;
using Sidebar.Core;

namespace Sidebar
{
    /// <summary>
    /// Interaction logic for LibraryWindow.xaml
    /// </summary>
    public partial class LibraryWindow : Window
    {
        private SidebarWindow ParentWindow;
        private WebClient DownloadWebClient;
        private CollectionView TileListCollectionView
        {
            get
            {
                return (CollectionView)CollectionViewSource.GetDefaultView(TileListView.ItemsSource);
            }
        }

        public LibraryWindow(SidebarWindow window)
        {
            InitializeComponent();

            ParentWindow = window;

            ItemsCount.Text = (string)Application.Current.TryFindResource("GettingTilesList");
            GetTileList();
        }

        private void DownloadButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (TileListView.SelectedIndex == -1)
            {
                return;
            }

            Directory.CreateDirectory(App.Settings.path + @"\Cache");

            // Reset progress status
            DownloadStatusTextBlock.Text = (string)Application.Current.TryFindResource("Connecting");
            DownloadProgressTextBlock.Text = "";
            DownloadProgressBar.Value = 0;

            LoadingGrid.Visibility = Visibility.Visible;

            DownloadStatusTextBlock.Text = (string)Application.Current.TryFindResource("DownloadingTile");

            TileMetadata metadata = (TileMetadata)TileListView.SelectedItem;
            string url = metadata.ResolvedDownloadUrl;

            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show((string)Application.Current.TryFindResource("ConnectionFailed"));
                LoadingGrid.Visibility = Visibility.Collapsed;
                return;
            }

            DownloadWebClient = new WebClient();
            DownloadWebClient.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(dowloader_DownloadFileCompleted);
            DownloadWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(dowloader_DownloadProgressChanged);
            DownloadWebClient.DownloadFileAsync(new Uri(url), App.Settings.path + @"\Cache\" + metadata.Name + ".tile");
        }

        private void dowloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadProgressBar.Value = e.ProgressPercentage;
            DownloadProgressTextBlock.Text = string.Format(
                (string)Application.Current.TryFindResource("DownloadProgress"),
                Math.Round((double)(e.BytesReceived / 1024)), Math.Round((double)(e.TotalBytesToReceive / 1024)));
        }

        private void dowloader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
            {
                // TODO: Move to tile/package manager
                string tileName = ((TileMetadata)TileListView.SelectedItem).Name;
                string tilePath = System.IO.Path.Combine(App.Settings.path, "Cache", tileName + ".tile");
                TaskDialogs.TileInstallDialog.ShowDialog(ParentWindow, tileName, tilePath);
            }
            else if (!e.Cancelled)
            {
                MessageBox.Show(e.Error.ToString(),
                    (string)Application.Current.TryFindResource("DownloadingFailed"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadingGrid.Visibility = Visibility.Collapsed;
        }

        private void CancelDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (DownloadWebClient.IsBusy)
                DownloadWebClient.CancelAsync();
        }

        private void GetTileList()
        {
            Directory.CreateDirectory(App.Settings.path + @"\Cache");

            using (WebClient client = new WebClient())
            {
                client.DownloadFileCompleted += (sender, e) =>
                {
                    if (e.Error != null)
                    {
                        ItemsCount.Text = e.Error.Message;
                        return;
                    }
                    if (!e.Cancelled)
                    {
                        ParseTileList();
                    }
                };
                client.DownloadFileAsync(
                    new Uri(Services.TileInfo), App.Settings.path + @"\Cache\Tiles.list");
            }
        }

        private void ParseTileList()
        {
            string file = App.Settings.path + @"\Cache\Tiles.list";

            if (Directory.Exists(App.Settings.path + @"\Cache") && File.Exists(file))
            {
                FileInfo f = new FileInfo(App.Settings.path + @"\Cache\Tiles.list");
                if (Math.Abs(DateTime.Now.Day - f.LastAccessTime.Day) > 3)
                {
                    f.Delete();
                    ParseTileList();
                    return;
                }

                TileListManifest manifest;
                using (StreamReader reader = new StreamReader(file))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(TileListManifest));
                    manifest = (TileListManifest)deserializer.Deserialize(reader);
                }

                if (manifest.ResolveUrls())
                {
                    TileListView.ItemsSource = manifest.Tiles;
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(TileListView.ItemsSource);
                    view.Filter = TileListFilter;
                }

                RefreshElementCount();
            }
        }

        private bool TileListFilter(object item)
        {
            TileMetadata metadata = (TileMetadata)item;
            string filterText = SearchField.Text.ToLower();

            if (string.IsNullOrEmpty(filterText))
            {
                return true;
            }

            if (metadata.Name.ToLower().Contains(filterText) ||
                metadata.Description.ToLower().Contains(filterText) ||
                metadata.Developer.ToLower().Contains(filterText))
            {
                return true;
            }

            return false;
        }

        private void RefreshElementCount()
        {
            string itemCount = (string)Application.Current.TryFindResource("ElementsCount");
            ItemsCount.Text = string.Format(itemCount, TileListView.Items.Count);
        }

        private void SearchField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TileListView != null && TileListView.ItemsSource != null)
            {
                TileListCollectionView.Refresh();
                RefreshElementCount();
            }
        }

        private void TileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TileMetadata currentItem = (TileMetadata)TileListView.SelectedItem;

            if (currentItem == null)
            {
                return;
            }

            CurrentItemTitle.Text = currentItem.Name;
            CurrentItemDescription.Text = currentItem.Description;
            CurrentItemDescription.ToolTip = currentItem.Description;
            CurrentItemAuthor.Text = currentItem.Developer;
            CurrentItemVersion.Text = currentItem.Version;
            CurrentItemIcon.Source = new BitmapImage(new Uri(currentItem.ResolvedIcon));

            if (DownloadButton.Visibility != Visibility.Visible)
            {
                DownloadButton.Visibility = Visibility.Visible;
            }
        }

        private void ItemIconImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Dispatcher.Invoke((Action)delegate
            {
                ((Image)sender).Source = new BitmapImage(
                    new Uri("/Sidebar;component/Resources/Tile_Icon.png", UriKind.Relative));
            }, null);
        }

        private void TileListView_GotFocus(object sender, RoutedEventArgs e)
        {
            ItemsCount.Visibility = Visibility.Collapsed;
            HeaderDetailPanel.Visibility = Visibility.Visible;
            MiscDetailPanel.Visibility = Visibility.Visible;
        }

        private void TileListView_LostFocus(object sender, RoutedEventArgs e)
        {
            ItemsCount.Visibility = Visibility.Visible;
            HeaderDetailPanel.Visibility = Visibility.Collapsed;
            MiscDetailPanel.Visibility = Visibility.Collapsed;

            CurrentItemIcon.Source = new BitmapImage(
                new Uri("/Sidebar;component/Resources/Library_icon.png", UriKind.Relative));
            DownloadButton.Visibility = Visibility.Collapsed;
        }

        private void SearchField_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false && string.IsNullOrEmpty(SearchField.Text))
            {
                SearchFieldPlaceholder.Visibility = Visibility.Visible;
                return;
            }
            SearchFieldPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void TileListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TileMetadata metadata = ((ListViewItem)sender).Content as TileMetadata;
            DownloadButton_MouseLeftButtonUp(metadata, e);
        }
    }
}
