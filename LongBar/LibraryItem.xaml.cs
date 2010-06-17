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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.IO;
using System.Net;

namespace LongBar
{
    /// <summary>
    /// Interaction logic for LibraryItem.xaml
    /// </summary>
    public partial class LibraryItem : UserControl
    {
        private bool _selected = false;

        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                if (value == true)
                    Bg1.Opacity = 0.7;
                else
                    Bg1.Opacity = 0;
            }
        }

        private string _icon;

        public string Icon
        {
            get { return _icon; }
            set 
            {
                _icon = value;

                ThreadStart threadStarter = delegate
                {
                    DownloadIcon();
                };
                Thread thread = new Thread(threadStarter);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }

        private string _header = "";

        public string Header
        {
            get { return _header; }
            set
            {
                ItemTitleTextBlock.Text = value;
                _header = value;
            }
        }

        public string Developer
        {
            get { return ItemDeveloperTextBlock.Text; }
            set { ItemDeveloperTextBlock.Text = value; }
        }

        public string Link;
        public string Description;
        public string Version;

        public LibraryItem()
        {
            InitializeComponent();
        }

        private void DownloadIcon()
        {
            string file = _icon.Substring(_icon.LastIndexOf("/") + 1);
            if (!Directory.Exists(LongBarMain.sett.path + @"\Cache") || !File.Exists(LongBarMain.sett.path + @"\Cache\" + file))
            {
                Directory.CreateDirectory(LongBarMain.sett.path + @"\Cache");

               try
                {
                    WebRequest request = WebRequest.Create(_icon);
                    WebResponse response = request.GetResponse();

                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string line = "";
                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLine();

                        if (line.Contains(@"\x2f" + Header + @".png\x3fpsid\x3d1', downloadUrl:"))
                        {
                            reader.Close();
                            response.Close();

                            line = line.Substring(line.IndexOf(@"\x2f" + Header + @".png\x3fpsid\x3d1', downloadUrl:") + (@"\x2f" + Header + @".png\x3fpsid\x3d1', downloadUrl:").Length + 2, line.IndexOf(@"\x2f" + Header + @".png\x3fdownload\x26psid\x3d1'") - line.IndexOf(@"\x2f" + Header + @".png\x3fpsid\x3d1', downloadUrl:") - 5);
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
                            client.DownloadFile(line, LongBarMain.sett.path + @"\Cache\" + file);
                            ItemIconImage.Dispatcher.BeginInvoke((Action)delegate
                            {
                                ItemIconImage.Source = new BitmapImage(new Uri(LongBarMain.sett.path + @"\Cache\" + file));
                            }, null);
                            reader.Close();
                            response.Close();
                            return;
                        }
                    }
                    reader.Close();
                    response.Close();
                    ItemIconImage.Dispatcher.Invoke((Action)delegate
                    {
                        ItemIconImage.Source = new BitmapImage(new Uri("/LongBar;component/Resources/Tile_Icon.png", UriKind.Relative));
                    }, null);
                }
                catch
                {
                    ItemIconImage.Dispatcher.Invoke((Action)delegate
                    {
                        ItemIconImage.Source = new BitmapImage(new Uri("/LongBar;component/Resources/Tile_Icon.png", UriKind.Relative));
                    }, null);
                }
            }
            else
            {
                if (File.Exists(LongBarMain.sett.path + @"\Cache\" + file))
                {
                    ItemIconImage.Dispatcher.Invoke((Action)delegate
                    {
                        ItemIconImage.Source = new BitmapImage(new Uri(LongBarMain.sett.path + @"\Cache\" + file));
                    }, null);
                }

                DirectoryInfo d = new DirectoryInfo(LongBarMain.sett.path + @"\Cache");
                if (Math.Abs(DateTime.Now.Day - d.CreationTime.Day) > 7)
                    try
                    {
                        d.Delete(true);
                    }
                    catch { }
            }
        }

        public string GeTileLink()
        {
            string header = Header;
            string line = "";
            //ThreadStart threadStarter = delegate
            //{

                string file = Link.Substring(Link.LastIndexOf("/") + 1);

                if (!Directory.Exists(LongBarMain.sett.path + @"\Cache"))
                    Directory.CreateDirectory(LongBarMain.sett.path + @"\Cache");

                    WebRequest request = WebRequest.Create(Link);
                    WebResponse response = request.GetResponse();

                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLine();

                        if (line.Contains(@"\x2f" + header + @".tile\x3fdownload\x26psid\x3d1', downloadUrl:"))
                        {
                            reader.Close();
                            response.Close();

                            line = line.Substring(line.IndexOf(@"\x2f" + header + @".tile\x3fdownload\x26psid\x3d1', downloadUrl:") + (@"\x2f" + header + @".tile\x3fdownload\x26psid\x3d1', downloadUrl:").Length + 2, line.IndexOf(@"\x2f" + header + @".tile\x3fdownload\x26psid\x3d1', demoteUrl:") - line.IndexOf(@"\x2f" + header + @".tile\x3fdownload\x26psid\x3d1', downloadUrl:") - 17);
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
                            reader.Close();
                            response.Close();
                            return line;
                        }
                    }
                    reader.Close();
                    response.Close();
                    return "";
            //};
            //Thread thread = new Thread(threadStarter);
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();
        }
    }
}
